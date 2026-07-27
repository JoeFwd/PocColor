using PocColor.Domain.Appearance.Model;

namespace PocColor.Domain.Appearance;

public interface IPlanCampaignBannerUseCase
{
    CampaignBannerChange Plan(CampaignBannerPolicy policy);
}
