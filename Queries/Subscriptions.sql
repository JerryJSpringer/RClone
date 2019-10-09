select UserInfo.Username, Community.Name
from UserInfo, Community, Subscription
where UserInfo.UserInfoId = Subscription.UserInfoId
	and Subscription.CommunityId = Community.CommunityId
order by UserInfo.Username, Community.Name	