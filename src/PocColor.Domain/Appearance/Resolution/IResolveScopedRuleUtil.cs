using PocColor.Domain.Appearance.Model;

namespace PocColor.Domain.Appearance.Resolution;

public interface IResolveScopedRuleUtil
{
    RuleOverride Resolve(AppearanceRuleSet rules, UnitProfile profile);
}
