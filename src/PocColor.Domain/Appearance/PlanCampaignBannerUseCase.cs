using PocColor.Domain.Appearance.Model;

namespace PocColor.Domain.Appearance;

public sealed class PlanCampaignBannerUseCase : IPlanCampaignBannerUseCase
{
    public CampaignBannerChange Plan(CampaignBannerPolicy policy)
    {
        bool followsAnyKingdomColor = policy.FollowKingdomColors
            || policy.FollowKingdomBackgroundOnly;

        return new CampaignBannerChange(
            policy.ConfiguredBannerCode,
            policy.CachedBannerCode,
            bannerCodeToRestore: followsAnyKingdomColor ? null : policy.CachedBannerCode,
            applyKingdomPrimaryColor: followsAnyKingdomColor,
            applyKingdomSecondaryColor: policy.FollowKingdomColors,
            configuredPrimaryColor: policy.ConfiguredPrimaryColor,
            configuredSecondaryColor: policy.ConfiguredSecondaryColor);
    }
}
