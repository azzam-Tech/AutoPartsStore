namespace AutoPartsStore.Core.Entities
{
    public class SystemSetting 
    {
        public int Id { get; private set; }
        public string SettingKey { get; private set; }
        public string SettingValue { get; private set; }
        public string? Description { get; private set; }
        public string? Category { get; private set; }

        public SystemSetting(string settingKey, string settingValue, string? description = null, string? category = null)
        {
            SettingKey = settingKey;
            SettingValue = settingValue;
            Description = description;
            Category = category;
        }

        public void UpdateValue(string newValue)
        {
            SettingValue = newValue;
        }
    }
}
