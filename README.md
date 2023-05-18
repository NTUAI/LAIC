# *LAIC* - League of Legends AI Commentator
[NTUAI Club](https://ntuai.club) x [NTU Esports Club](https://www.facebook.com/ntuesports)


LAIC (英雄聯盟智慧評論員) 系統主要包含：

- 用戶介面： 負責與使用者進行互動。
- 資訊擷取： 負責從各種不同的資源中收集相關資訊。
- 生成式人工智慧： 作為系統的核心部分，對收集到的資訊進行分析、歸納，並生成有意義的回答。
- 語音合成： 將生成的回答轉化為語音，方便使用者聆聽。

The **LAIC** (League AI Commentator) system comprises:

- User Interface: The interface for interaction with users.
- Information Extraction: Collects relevant data from various sources.
- Generative AI: The core component that analyzes and summarizes the collected information to generate meaningful responses.
- Speech Synthesis: Converts the generated responses into audio for user convenience.
_ _ _
![image](https://github.com/NTUAI/LAIC/assets/41275553/5a122a67-7bb7-44a6-beb4-a13c569427ea)

_ _ _

**英雄聯盟智慧評論員系統**的主要功能與說明如下：

- **系統架設：** 該系統是使用 ASP.NET 所架設的網站。

- **資料獲取：** 利用 Riot API 獲取對戰資訊。

- **評論生成：** 透過 ChatGPT 將 Riot API 的資訊與影片音訊結合，自動生成具有深度的評論內容。

- **字幕：** 提供中英文字幕切換功能。

- **語音：** 提供三種風格的語音播報。

- **影片來源：** 支持從 YouTube 擷取影片、上傳影片，或使用比賽的 MatchID。

- **賽事資訊：**
  - 提供一周賽事清單。
  - 提供視覺化的對戰資訊。
  - 顯示賽事表。

- **參考資源：**
  - Riot Games API: [https://developer.riotgames.com/groups](https://developer.riotgames.com/groups)
  - AdminLTE: [https://github.com/ColorlibHQ/AdminLTE](https://github.com/ColorlibHQ/AdminLTE)
  - OpenAI API Application: [https://platform.openai.com/account/api-keys](https://platform.openai.com/account/api-keys)
  - Azure Text to Speech: [https://azure.microsoft.com/zh-tw/products/cognitive-services/text-to-speech](https://azure.microsoft.com/zh-tw/products/cognitive-services/text-to-speech)
_ _ _
# 前端頁面顯示

![網頁擷取_14-5-2023_12115_localhost](https://github.com/NTUAI/LAIC/assets/96654161/6929a493-f7e4-4b32-ac2b-69c01a1e344d)
