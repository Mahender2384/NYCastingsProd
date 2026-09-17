using System.Collections.Generic;
using NYCastings.API.Core.Models.HideUnhideTalentModel;
using NYCastings.API.Core.Models.ProfileSearchModel;

namespace NYCastings.API.Core.Contracts.ProfileSearchInterface;

public interface IProfileSearchInterface
{
	IEnumerable<SearchProfileModel> SearchProfiles(SearchProfileRequest request);

	IEnumerable<SearchProfileModel> GetHidedTalentDetails(int clientId);

	bool HideOrUnhideTalent(HideUnhideTalentRequest request);
}
