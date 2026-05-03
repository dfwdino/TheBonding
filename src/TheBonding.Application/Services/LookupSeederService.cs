using TheBonding.Application.Interfaces.Repositories;
using TheBonding.Application.Interfaces.Services;
using TheBonding.Domain.Entities;

namespace TheBonding.Application.Services;

/// <summary>
/// Seeds default encrypted lookup item values at first-launch setup.
/// Called by AuthService.SetupAsync after Key2 is available.
///
/// All values are encrypted before insertion.
/// Users can edit or delete any default item after setup.
/// </summary>
public sealed class LookupSeederService : ILookupSeederService
{
    private readonly ILookupRepository  _lookupRepo;
    private readonly IEncryptionService _encryption;

    // Default values keyed by category name.
    // These are encrypted with Key2 at first-launch and inserted into LookupItem.
    private static readonly Dictionary<string, string[]> Defaults = new()
    {
        ["Activity Type"] =
        [
            "Intercourse",
            "Oral (giving)",
            "Oral (receiving)",
            "Manual (giving)",
            "Manual (receiving)",
            "Anal",
            "Cuddling / Intimacy",
            "Massage",
            "Other"
        ],
        ["Relationship Type"] =
        [
            "Partner",
            "Spouse",
            "Casual",
            "Friend with Benefits",
            "Online / Long Distance",
            "Other"
        ],
        ["Contraception"] =
        [
            "Condom",
            "Birth Control Pill",
            "IUD",
            "Implant",
            "Patch",
            "Ring",
            "Diaphragm",
            "None",
            "Other"
        ],
        ["Location"] =
        [
            "Home",
            "Partner's Home",
            "Hotel / Motel",
            "Car",
            "Outdoors",
            "Other"
        ],
        ["Mood"] =
        [
            "Romantic",
            "Passionate",
            "Playful",
            "Adventurous",
            "Relaxed",
            "Spontaneous",
            "Other"
        ],
        ["Gender Identity"] =
        [
            "Man",
            "Woman",
            "Non-binary",
            "Genderqueer",
            "Genderfluid",
            "Agender",
            "Transgender Man",
            "Transgender Woman",
            "Other",
            "Prefer not to say"
        ],
        ["Sexual Orientation"] =
        [
            "Straight / Heterosexual",
            "Gay / Lesbian",
            "Bisexual",
            "Pansexual",
            "Asexual",
            "Queer",
            "Other",
            "Prefer not to say"
        ],
        ["Health Test Type"] =
        [
            "STI Full Panel",
            "HIV",
            "Gonorrhea",
            "Chlamydia",
            "Syphilis",
            "Herpes (HSV)",
            "HPV",
            "Hepatitis B",
            "Hepatitis C",
            "Other"
        ],
        ["Health Test Result"] =
        [
            "Negative",
            "Positive",
            "Inconclusive",
            "Pending",
            "Not Tested Yet"
        ],
        ["Time of Day"] =
        [
            "Morning",
            "Afternoon",
            "Evening",
            "Late Night"
        ],
        ["Position"] =
        [
            "Missionary",
            "Doggy Style",
            "Cowgirl",
            "Reverse Cowgirl",
            "Spooning",
            "Standing",
            "Sitting",
            "Other"
        ],
        ["Role"] =
        [
            "Dominant",
            "Submissive",
            "Switch",
            "Other"
        ],
        ["Climax"] =
        [
            "Me",
            "Partner",
            "Both",
            "Neither"
        ]
    };

    public LookupSeederService(ILookupRepository lookupRepo, IEncryptionService encryption)
    {
        _lookupRepo  = lookupRepo;
        _encryption  = encryption;
    }

    public async Task SeedDefaultsAsync(byte[] key2)
    {
        var categories = await _lookupRepo.GetCategoriesAsync();

        foreach (var category in categories)
        {
            // Skip if items already exist — safe to call multiple times
            if (await _lookupRepo.ItemsExistForCategoryAsync(category.Id)) continue;

            if (!Defaults.TryGetValue(category.Name, out var values)) continue;

            for (var i = 0; i < values.Length; i++)
            {
                var item = new LookupItem
                {
                    CategoryId     = category.Id,
                    EncryptedValue = _encryption.EncryptString(values[i], key2),
                    SortOrder      = i,
                    IsDefault      = true
                };

                await _lookupRepo.CreateItemAsync(item);
            }
        }
    }
}
