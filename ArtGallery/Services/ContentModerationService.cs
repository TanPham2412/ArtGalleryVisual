using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ArtGallery.Models;
using Microsoft.EntityFrameworkCore;

namespace ArtGallery.Services
{
    public interface IContentModerationService
    {
        (bool isValid, string errorMessage) ValidateContent(string content);
        Task<(bool isValid, string errorMessage)> ValidateCommentSpamAsync(string content, int artworkId, string userId);
    }

    public class ContentModerationService : IContentModerationService
    {
        private readonly List<string> _sensitiveWords;
        private readonly int _maxRepeatedSequenceLength;
        private readonly ArtGalleryContext _context;
        private readonly int _maxSimilarCommentsPerArtwork = 3; // Số lượng bình luận tương tự tối đa cho phép trên một bài viết

        public ContentModerationService(ArtGalleryContext context)
        {
            _context = context;
            
            // Danh sách các từ ngữ nhạy cảm cần kiểm duyệt
            _sensitiveWords = new List<string>
            {
                "tự tử", "chết", "giết", "nạn nhân", "tội phạm", 
                "tử vong", "lừa đảo", "thuốc lá", "đạn", "ma túy", "dao", "cần sa", "tự sát"
            };

            // Độ dài tối đa cho phép của một chuỗi ký tự lặp lại
            _maxRepeatedSequenceLength = 3;
        }

        public (bool isValid, string errorMessage) ValidateContent(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return (true, string.Empty);

            // Kiểm tra từ ngữ nhạy cảm
            foreach (var word in _sensitiveWords)
            {
                if (content.ToLower().Contains(word.ToLower()))
                {
                    return (false, "Nội dung bình luận chứa từ ngữ nhạy cảm");
                }
            }

            // Kiểm tra spam (ký tự lặp lại nhiều lần)
            if (ContainsRepeatedCharacters(content, _maxRepeatedSequenceLength))
            {
                return (false, "Nội dung bình luận có dấu hiệu spam (ký tự lặp lại nhiều lần)");
            }

            // Kiểm tra spam (từ lặp lại nhiều lần)
            if (ContainsRepeatedWords(content, _maxRepeatedSequenceLength))
            {
                return (false, "Nội dung bình luận có dấu hiệu spam (từ lặp lại nhiều lần)");
            }

            return (true, string.Empty);
        }

        private bool ContainsRepeatedCharacters(string content, int maxRepeats)
        {
            // Kiểm tra chuỗi ký tự lặp lại
            for (int i = 0; i < content.Length - maxRepeats; i++)
            {
                char c = content[i];
                int count = 1;

                for (int j = i + 1; j < content.Length && content[j] == c; j++)
                {
                    count++;
                }

                if (count > maxRepeats)
                {
                    return true;
                }
            }

            return false;
        }

        private bool ContainsRepeatedWords(string content, int maxRepeats)
        {
            // Tách nội dung thành các từ
            var words = Regex.Split(content.ToLower(), @"\W+")
                .Where(w => !string.IsNullOrWhiteSpace(w))
                .ToList();

            // Đếm số lần xuất hiện của mỗi từ
            var wordCounts = new Dictionary<string, int>();
            foreach (var word in words)
            {
                if (word.Length >= 2) // Chỉ xét các từ có độ dài >= 2
                {
                    if (wordCounts.ContainsKey(word))
                    {
                        wordCounts[word]++;
                    }
                    else
                    {
                        wordCounts[word] = 1;
                    }
                }
            }

            // Kiểm tra xem có từ nào lặp lại quá nhiều lần không
            return wordCounts.Any(kv => kv.Value > maxRepeats);
        }
        
        public async Task<(bool isValid, string errorMessage)> ValidateCommentSpamAsync(string content, int artworkId, string userId)
        {
            // Trước tiên kiểm tra nội dung theo các quy tắc thông thường
            var basicValidation = ValidateContent(content);
            if (!basicValidation.isValid)
            {
                return basicValidation;
            }
            
            // Lấy danh sách các bình luận hiện có của bài viết
            var existingComments = await _context.BinhLuans
                .Where(c => c.MaTranh == artworkId)
                .ToListAsync();
                
            // Đếm số lượng bình luận có nội dung tương tự từ cùng người dùng
            int similarCommentsCount = existingComments
                .Count(c => c.MaNguoiDung == userId && IsSimilarContent(c.NoiDung, content));
                
            if (similarCommentsCount >= _maxSimilarCommentsPerArtwork)
            {
                return (false, "Bạn đã đăng quá nhiều bình luận tương tự trên bài viết này");
            }
            
            // Kiểm tra xem có nhiều người dùng đăng cùng một nội dung không
            var usersWithSimilarComments = existingComments
                .Where(c => IsSimilarContent(c.NoiDung, content))
                .Select(c => c.MaNguoiDung)
                .Distinct()
                .Count();
                
            if (usersWithSimilarComments >= _maxSimilarCommentsPerArtwork)
            {
                return (false, "Nội dung bình luận này đã được đăng quá nhiều lần bởi nhiều người dùng");
            }
            
            return (true, string.Empty);
        }
        
        private bool IsSimilarContent(string content1, string content2)
        {
            // Chuẩn hóa nội dung để so sánh
            content1 = NormalizeContent(content1);
            content2 = NormalizeContent(content2);
            
            // Kiểm tra nội dung giống hệt nhau
            if (content1 == content2)
            {
                return true;
            }
            
            // Kiểm tra độ tương đồng đơn giản (có thể mở rộng với thuật toán phức tạp hơn)
            // Ví dụ: Levenshtein distance hoặc các thuật toán so khớp văn bản khác
            double similarity = CalculateSimilarity(content1, content2);
            return similarity > 0.8; // Ngưỡng tương đồng 80%
        }
        
        private string NormalizeContent(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return string.Empty;
                
            // Chuyển về chữ thường và loại bỏ khoảng trắng thừa
            return Regex.Replace(content.ToLower(), @"\s+", " ").Trim();
        }
        
        private double CalculateSimilarity(string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2))
                return 0;
                
            // Thuật toán Levenshtein distance đơn giản
            int[,] distance = new int[s1.Length + 1, s2.Length + 1];
            
            for (int i = 0; i <= s1.Length; i++)
                distance[i, 0] = i;
            for (int j = 0; j <= s2.Length; j++)
                distance[0, j] = j;
                
            for (int i = 1; i <= s1.Length; i++)
            {
                for (int j = 1; j <= s2.Length; j++)
                {
                    int cost = (s1[i - 1] == s2[j - 1]) ? 0 : 1;
                    distance[i, j] = Math.Min(
                        Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                        distance[i - 1, j - 1] + cost);
                }
            }
            
            // Tính toán độ tương đồng dựa trên khoảng cách Levenshtein
            int maxLength = Math.Max(s1.Length, s2.Length);
            if (maxLength == 0) return 1.0; // Hai chuỗi rỗng được coi là giống nhau
            
            return 1.0 - (double)distance[s1.Length, s2.Length] / maxLength;
        }
    }
}