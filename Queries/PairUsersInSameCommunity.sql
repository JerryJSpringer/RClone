select distinct Community.Name, U1.UserInfoId, U1.Username, U2.UserInfoId, U2.Username

from UserInfo U1, UserInfo U2, Subscription, Community

where U1.UserInfoId in 
	(select UserInfo.UserInfoId 
	from UserInfo, Subscription, Community 
	where UserInfo.UserInfoId = Subscription.UserInfoId and Subscription.CommunityId = Community.CommunityId)
	
	and U2.UserInfoId in
	(select UserInfo.UserInfoId 
	from UserInfo, Subscription, Community 
	where UserInfo.UserInfoId = Subscription.UserInfoId and Subscription.CommunityId = Community.CommunityId)

	and U1.UserInfoId < U2.UserInfoId

order by Community.Name, U1.UserInfoId, U2.UserInfoId