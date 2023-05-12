import re
from googletrans import Translator
import json
import csv
import opencc
import shutil
import requests
import langid
import openai
import pandas as pd
from hanziconv import HanziConv


with open('G:\\NTU\\getheroinf\\herolist.json', 'r', encoding='utf_8_sig') as f:
    js_content = f.read()

js_dict = json.loads(js_content)

for key, value in js_dict.items():
    if isinstance(value, str):
        js_dict[key] = value.replace("\\", "")


with open('herolist.csv', 'w', newline='') as f:
    writer = csv.writer(f)
    for key, value in js_dict.items():
        if isinstance(value, list):
            for v in value:
                if any('\u4e00' <= c <= '\u9fff' for c in v):
                    v = opencc.convert(v, config='s2t.json')
                writer.writerow([key, v])
        else:
            if any('\u4e00' <= c <= '\u9fff' for c in value):
                value = opencc.convert(value, config='s2t.json')
            writer.writerow([key, value])
################################
converter = opencc.OpenCC('s2t')  # 創建 OpenCC 實例

with open('herolist.csv', 'r', newline='', encoding='utf-8') as infile:
    reader = csv.reader(infile)
    with open('herolistw.csv', 'w', newline='', encoding='utf-8') as outfile:
        writer = csv.writer(outfile)
        for row in reader:
            translated_row = []
            for cell in row:
                if any('\u4e00' <= c <= '\u9fff' for c in cell):  # 判斷是否包含漢字
                    cell = converter.convert(cell)  # 翻譯
                translated_row.append(cell)
            writer.writerow(translated_row)
#############


def scrape_tournament_data(start_tour, end_tour):
    with open('tournament_data.csv', 'a+', newline='') as csvfile:
        writer = csv.writer(csvfile)
        writer.writerow([
            'Team Name', 'Team Full Name', 'Team ID', 'Appear', 'Win',
            'Win Rate', 'Duration per Game', 'Duration per Win',
            'Duration per Lose', 'Kill', 'Dead', 'Bloodiness',
            'Minions Percent per Game', 'First Blood Percent',
            'First Tower Percent', 'First Dragon Percent',
            'First Baron Nashor Percent', 'Dragon Percent',
            'Baron Nashor Percent', 'Rift Hearld Percent',
            'Ten Minutes Gold Offset per Game', 'Damage per Minute',
            'Tank per Minute', 'Put Eye per Minute',
            'Destroy Eye per Minute', 'Buy True Eye per Minute', 'Lose'
        ])

        for tour in range(start_tour, end_tour + 1):
            url = f'http://lol.admin.pentaq.com/api/tournament_team_data?tour={tour}&patch='
            response = requests.get(url)

            try:
                data = response.json()

                if data['res'] == True:
                    teams_data = data['data']['teams_data']
                    for team in teams_data:
                        row = [
                            team['team_name'], team['team_full_name'],
                            team['team_id'], team['appear'], team['win'],
                            team['win_rate'], team['duration_per_game'],
                            team['duration_per_win'], team['duration_per_lose'],
                            team['kill'], team['dead'], team['bloodiness'],
                            team['minions_percent_per_game'], team['first_blood_percent'],
                            team['first_tower_percent'], team['first_dragon_percent'],
                            team['first_baron_nashor_percent'], team['dragon_percent'],
                            team['baron_nashor_percent'], team['rift_hearld_percent'],
                            team['ten_minutes_gold_offset_per_game'], team['damage_per_minute'],
                            team['tank_per_minute'], team['put_eye_per_minute'],
                            team['destroy_eye_per_minute'], team['buy_true_eye_per_minute'],
                            team['lose']
                        ]
                        writer.writerow(row)

            except Exception as e:
                print(f'Error: {e}')


scrape_tournament_data(1, 1000)  # 遍歷 tour=1~10 的資料
