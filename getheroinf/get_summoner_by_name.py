from riotwatcher import LolWatcher, ApiError

#Fill in your Riot API key and default region
api_key = 'RGAPI-xxxxxxxxxxxxx'
my_region = 'tw2'

# Create a LolWatcher instance
lol_watcher = LolWatcher(api_key)

# Call the get_summoner_by_name function, passing in the summoner name
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
