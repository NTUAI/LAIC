from riotwatcher import LolWatcher, ApiError

# 填入您的 Riot API 密钥和默认区域
api_key = 'RGAPI-1f6b2834-d08c-4ec8-bbfc-9e5c46c3acd2'
my_region = 'tw2'

# 创建 LolWatcher 实例
lol_watcher = LolWatcher(api_key)

# 调用 get_summoner_by_name 函数，传入召唤师名称
try:
    summoner = lol_watcher.summoner.by_name(my_region, 'summoner_name')
    print('summoner ID：', summoner['id'])
    print('summoner name：', summoner['name'])
    print('summoner level：', summoner['summonerLevel'])
except ApiError as e:
    if e.response.status_code == 404:
        print('not find summoner')
    else:
        print('req false：', e.response.status_code, e.response.text)
