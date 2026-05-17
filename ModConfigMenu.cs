using ContentPatcher;
using StardewModdingAPI;

namespace UnlimitedEventExpansion
{

    public partial class ModEntry
    {

        public static void ConfigMenu(IContentPatcherAPI api, IManifest modManifest, IModHelper helper)
        {
            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = helper.ModRegistry.GetApi<UnlimitedEventExpansion.Data.IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            // register mod
            configMenu.Register(
                mod: modManifest,
                reset: () => Config = new ModConfig(),
                save: () => helper.WriteConfig(Config),
                titleScreenOnly: true
            );

            configMenu.AddParagraph(
                mod: modManifest,
                text: () => "All options in this menu require OpenAI key provided to be effective. Without a key, the mod will use a shared key with stricter limits, and the options below won't have any effect."
            );


            configMenu.AddSectionTitle(
                mod: modManifest,
                text: () => "OpenAI setup"
            );

            configMenu.AddTextOption(
                mod: modManifest,
                getValue: () => Config?.OpenAIKey ?? "",
                setValue: value => Config.OpenAIKey = value?.Trim() ?? "",
                name: () => "OpenAI API key",
                tooltip: () => "Use your own key to remove shared usage limits.\nGet one from https://platform.openai.com/account/api-keys."
            );

            configMenu.AddTextOption(
                mod: modManifest,
                name: () => "Model",
                tooltip: () => "Choose the OpenAI model to use when your own key is set.",
                getValue: () => Config.OpenAIModel,
                setValue: value => Config.OpenAIModel = value,
                allowedValues: new string[]
                {
                    ModConfig.OpenAIModel_51,
                    ModConfig.OpenAIModel_54mini,
                    ModConfig.OpenAIModel_54nano,
                    ModConfig.OpenAIModel_5mini,
                    ModConfig.OpenAIModel_5nano
                },
                formatAllowedValue: FormatOpenAIModel
            );

            configMenu.AddSectionTitle(
                mod: modManifest,
                text: () => "Event generation"
            );

            configMenu.AddParagraph(
                mod: modManifest,
                text: () => "These options control event quality and pacing when your own OpenAI key is configured."
            );

            configMenu.AddBoolOption(
                mod: modManifest,
                name: () => "Ignore heart-level requirements",
                tooltip: () => "If enabled, events can trigger without the usual heart-level checks.\nRestart the game after changing this option.",
                getValue: () => Config.AllowEarlyEvent,
                setValue: value => Config.AllowEarlyEvent = value
            );

            configMenu.AddTextOption(
                mod: modManifest,
                name: () => "Event length",
                tooltip: () => "Controls how much event dialogue is generated per heart level.",
                getValue: () => Config.EventLength,
                setValue: value => Config.EventLength = value,
                allowedValues: new string[]
                {
                    ModConfig.EventLengthShort,
                    ModConfig.EventLengthMedium,
                    ModConfig.EventLengthLong,
                    ModConfig.EventLengthExtraLong
                },
                formatAllowedValue: FormatEventLength
            );

            configMenu.AddTextOption(
                mod: modManifest,
                name: () => "NPC detail level",
                tooltip: () => "Higher detail gives richer NPC personality at higher token cost.\nCompared to Standard: Minimal uses about 100 fewer tokens, Detailed uses about 100 extra tokens per NPC.",
                getValue: () => Config.CharacteristicMode,
                setValue: value => Config.CharacteristicMode = value,
                allowedValues: new string[]
                {
                    ModConfig.CharacteristicModeMinimal,
                    ModConfig.CharacteristicModeShort,
                    ModConfig.CharacteristicModeLong
                },
                formatAllowedValue: FormatCharacteristicMode
            );
        }

        private static string FormatOpenAIModel(string value)
        {
            return value switch
            {
                ModConfig.OpenAIModel_51 => "GPT-5.1 (best quality, higher cost)",
                ModConfig.OpenAIModel_5mini => "GPT-5 Mini (balanced)",
                ModConfig.OpenAIModel_5nano => "GPT-5 Nano (lowest cost)",
                ModConfig.OpenAIModel_54mini => "GPT-5.4 Mini (balanced, newer)",
                ModConfig.OpenAIModel_54nano => "GPT-5.4 Nano (low cost, newer)",
                _ => value
            };
        }

        private static string FormatEventLength(string value)
        {
            return value switch
            {
                ModConfig.EventLengthShort => "Short (up to 10 lines)",
                ModConfig.EventLengthMedium => "Medium (up to 12 lines)",
                ModConfig.EventLengthLong => "Long (up to 15 lines)",
                ModConfig.EventLengthExtraLong => "Extra long (up to 20 lines)",
                _ => value
            };
        }

        private static string FormatCharacteristicMode(string value)
        {
            return value switch
            {
                ModConfig.CharacteristicModeMinimal => "Minimal (lower token use)",
                ModConfig.CharacteristicModeShort => "Standard (recommended)",
                ModConfig.CharacteristicModeLong => "Detailed (higher token use)",
                _ => value
            };
        }

    }

}