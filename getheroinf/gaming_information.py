

import pandas as pd
import json
import opencc
import requests
from faker import Factory
from bs4 import BeautifulSoup
# pip install beautifulsoup4
f = Factory.create()


def get_all_heros():
    url = 'https://game.gtimg.cn/images/lol/act/img/js/heroList/hero_list.js'
    headers = {
        'user-agent': f.user_agent()
    }
    r = requests.get(url, headers=headers)
    r.encoding = r.apparent_encoding
    c = r.text
    l = json.loads(c)['hero']

    # 将数据转换为dataframe
    df = pd.DataFrame(l)
    df = df[['heroId', 'name', 'alias']]
    df.columns = ['ID', '姓名', '别名']

    # 将簡體中文转换为繁體中文
    cc = opencc.OpenCC('s2t.json')
    df['姓名'] = df['姓名'].apply(lambda x: cc.convert(x))
    df['别名'] = df['别名'].apply(lambda x: cc.convert(x))

    # 将dataframe保存到csv文件中
    df.to_csv('heros.csv', index=False, encoding='utf-8-sig')


if __name__ == '__main__':
    get_all_heros()
