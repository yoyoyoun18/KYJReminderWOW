using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace KimYoungJoReminder.Models
{
    /// <summary>
    /// Boss Profile JSON 파일 관리
    /// </summary>
    public class ProfileManager
    {
        /// <summary>
        /// 프로필 저장 폴더 경로
        /// </summary>
        private static readonly string PROFILES_FOLDER = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "KimYoungJoReminder",
            "Profiles"
        );

        /// <summary>
        /// Boss 이름을 파일명으로 변환
        /// </summary>
        private string GetProfileFileName(string bossName)
        {
            // "Boss 1" -> "boss1.json"
            return bossName.Replace(" ", "").ToLower() + ".json";
        }

        /// <summary>
        /// 프로필 파일 전체 경로
        /// </summary>
        private string GetProfileFilePath(string bossName)
        {
            return Path.Combine(PROFILES_FOLDER, GetProfileFileName(bossName));
        }

        /// <summary>
        /// 프로필 저장 폴더 생성 (없으면)
        /// </summary>
        private void EnsureProfileFolderExists()
        {
            if (!Directory.Exists(PROFILES_FOLDER))
            {
                Directory.CreateDirectory(PROFILES_FOLDER);
            }
        }

        /// <summary>
        /// 프로필 저장
        /// </summary>
        public void SaveProfile(string bossName, List<ReminderItem> reminders)
        {
            EnsureProfileFolderExists();

            var profile = new BossProfile
            {
                Name = bossName,
                Reminders = reminders.Select(r => new ReminderItemData
                {
                    TimeInSeconds = r.TimeInSeconds,
                    Duration = r.Duration,
                    Text = r.Text
                }).ToList()
            };

            string json = JsonConvert.SerializeObject(profile, Formatting.Indented);
            string filePath = GetProfileFilePath(bossName);

            File.WriteAllText(filePath, json);
        }

        /// <summary>
        /// 프로필 로드
        /// </summary>
        public List<ReminderItem> LoadProfile(string bossName)
        {
            string filePath = GetProfileFilePath(bossName);

            // 파일이 없으면 빈 리스트 반환
            if (!File.Exists(filePath))
            {
                return new List<ReminderItem>();
            }

            try
            {
                string json = File.ReadAllText(filePath);
                var profile = JsonConvert.DeserializeObject<BossProfile>(json);

                if (profile == null || profile.Reminders == null)
                {
                    return new List<ReminderItem>();
                }

                // ReminderItemData → ReminderItem 변환
                return profile.Reminders.Select(r => new ReminderItem(
                    r.TimeInSeconds,
                    r.Text,
                    r.Duration
                )).ToList();
            }
            catch (Exception ex)
            {
                // JSON 파싱 실패 시 빈 리스트 반환
                Console.WriteLine($"Failed to load profile '{bossName}': {ex.Message}");
                return new List<ReminderItem>();
            }
        }

        /// <summary>
        /// 모든 프로필 파일 목록 가져오기
        /// </summary>
        public List<string> GetAllProfileNames()
        {
            if (!Directory.Exists(PROFILES_FOLDER))
            {
                return new List<string>();
            }

            return Directory.GetFiles(PROFILES_FOLDER, "*.json")
                .Select(Path.GetFileNameWithoutExtension)
                .Select(name =>
                {
                    // "boss1" -> "Boss 1"
                    if (name.StartsWith("boss"))
                    {
                        var num = name.Substring(4);
                        return $"Boss {num}";
                    }
                    return name;
                })
                .ToList();
        }

        /// <summary>
        /// 프로필 삭제
        /// </summary>
        public bool DeleteProfile(string bossName)
        {
            string filePath = GetProfileFilePath(bossName);

            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }
    }
}
