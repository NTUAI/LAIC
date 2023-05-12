import json
import csv

# Open the input JSON file
with open('championFull.json', 'r',  encoding='utf_8_sig') as json_file:
    # Load the JSON content
    json_content = json.load(json_file)

# Remove backslashes from JSON keys and values
json_content = json.loads(json.dumps(json_content).replace('\\', ''))

# Open the output CSV file and create a CSV writer
with open('heroes.csv', 'w',  encoding='utf_8_sig', newline='') as csv_file:
    writer = csv.writer(csv_file)

    # Write the header row
    writer.writerow(['heroId', 'name', 'alias', 'title', 'roles', 'isWeekFree', 'attack', 'defense', 'magic', 'difficulty', 'selectAudio', 'banAudio',
                    'isARAMweekfree', 'ispermanentweekfree', 'changeLabel', 'goldPrice', 'couponPrice', 'camp', 'campId', 'keywords', 'instance_id'])

    # Write each hero as a row in the CSV file
    for hero in json_content['hero']:
        writer.writerow([hero['heroId'], hero['name'], hero['alias'], hero['title'], ','.join(hero['roles']), hero['isWeekFree'], hero['attack'], hero['defense'], hero['magic'], hero['difficulty'], hero['selectAudio'],
                        hero['banAudio'], hero['isARAMweekfree'], hero['ispermanentweekfree'], hero['changeLabel'], hero['goldPrice'], hero['couponPrice'], hero['camp'], hero['campId'], hero['keywords'], hero['instance_id']])
