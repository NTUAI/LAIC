# -*- coding: utf-8 -*-
"""
Created on Sat Mar  6 21:18:12 2021

@author: compute
IDEA= Find the number of mid, top, etc. players in challenger and what their most played champion is
"""
import cassiopeia as cass
import roleml
import json
from datetime import timedelta
import time
from collections import Counter


def getAPI_key():
    f = 'RGAPI-8f2c61c3-593f-4d5e-a337-0adabcf77afd'
    return f


def analyzeMatch(match, summoner):
    p = match.participants[summoner]

    roleml.change_role_formatting('full')
    match.timeline.load()

    roleml.predict(match.to_dict(), match.timeline.to_dict(), True)
    roleml.add_cass_predicted_roles(match)
    role = p.predicted_role

    # role= p.role.value
    champ = champList[str(p.champion.id)]

    return role, champ


def get_challenger_data():
    data = cass.get_challenger_league(
        cass.Queue.ranked_solo_fives)  # get the challenger data

    summonerNames = []
    role_dict = []
    champ_dict = []
    for item in data.entries:
        if data.entries.index(item) % 10 == 0:
            # print every 10 entries to update user
            print('\nCURRENT LADDER ENTRY= ', data.entries.index(item))
        elif data.entries.index(item) > 5:
            pass
        else:
            summonerNames.append(item.summoner.name)
            summoner = item.summoner
            match_history = summoner.match_history(queues={cass.Queue.ranked_solo_fives},
                                                   begin_index=0, end_index=4)
            roles = []
            champs = []
            for match in match_history:
                if match.is_remake:
                    pass
                elif match.duration < timedelta(minutes=15, seconds=30):
                    # skip ff at 15
                    pass
                else:
                    # now we want the role [top, mid, jg, adc, support] and champion
                    # keep track of each one per match to find most common
                    role, champ = analyzeMatch(match, summoner)
                    roles.append(role)
                    champs.append(champ)

            main_role = Counter(roles).most_common(1)[0]
            main_champ = Counter(champs).most_common(1)[0]

            role_dict.append(main_role)
            champ_dict.append(main_champ)

    return summonerNames, role_dict, champ_dict


def write_output(summonerNames, role_dict, champ_dict):
    # summoner name, (role, games), (champ, games)
    print("\nSummoner name, (Role, games), (Champ, games)")
    for i in range(0, len(summonerNames)):
        print(summonerNames[i], "//", role_dict[i], "//", champ_dict[i])


# %% Main run
if __name__ == "__main__":

    start_time = time.time()

    # or replace with your own api key
    cass.set_riot_api_key('RGAPI-8f2c61c3-593f-4d5e-a337-0adabcf77afd')
    cass.set_default_region("TW")  # or replace with another region

    with open('G:\\NTU\\championFull.json', 'r', encoding='utf-8-sig') as champList_file:
        champList = json.load(champList_file)
        champList_file.close()
        champList = champList['keys']

    summonerNames, role_dict, champ_dict = get_challenger_data()
    write_output(summonerNames, role_dict, champ_dict)

    print("\n--- %s seconds ---" % (time.time() - start_time))
# %%
