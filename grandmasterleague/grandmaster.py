from pandas.io.json import json_normalize
import requests
import pandas as pd
import numpy as np
import time
import warnings

warnings.simplefilter(action='ignore', category=FutureWarning)
my_api = 'RGAPI-8f2c61c3-593f-4d5e-a337-0adabcf77afd'

grmaster = 'https://euw1.api.riotgames.com/lol/league/v4/grandmasterleagues/by-queue/RANKED_SOLO_5x5?api_key=' + my_api
req = requests.get(grmaster)
grmaster_df = json_normalize(req.json()['entries'])

grmaster_df = grmaster_df.sort_values('leaguePoints', ascending=False)


summoner_df = pd.DataFrame()

for i in range(len(grmaster_df)):
    summoner = 'https://euw1.api.riotgames.com/lol/summoner/v4/summoners/{}?api_key='.\
        format(grmaster_df['summonerId'].iloc[i]) + my_api

    req = requests.get(summoner)
    print(req.status_code)

    if req.status_code == 200:
        pass
    elif req.status_code == 429:
        while True:  # while loop because of riot api cost
            if req.status_code == 429:  # 429 error is api cost issue
                print('429 delay try 10 second')  # approximate 110 second wait
                time.sleep(10)

                summoner = 'https://euw1.api.riotgames.com/lol/summoner/v4/summoners/{}?api_key='.\
                    format(grmaster_df['summonerId'].iloc[i]) + my_api

                req = requests.get(summoner)
                print(req.status_code)

            elif req.status_code == 200:
                print('limit cost resolve')
                break

    summoner_df0 = json_normalize(req.json())

    summoner_df = summoner_df.append(summoner_df0)

    grmaster_user = pd.merge(summoner_df,
                             grmaster_df.loc[:, [
                                 'summonerId', 'leaguePoints', 'rank', 'wins', 'losses']],
                             left_on='id', right_on='summonerId', how='left').drop(columns='summonerId')
#limit api rate
    for i in range(len(grmaster_user)):
        with open('grmasteruser.csv', 'w', encoding="utf-8") as f:
            f.write(grmaster_user.to_csv())
        if (i+1) % 20 == 0:
            time.sleep(1)
        elif (i+1) % 100 == 0:
            time.sleep(120)
