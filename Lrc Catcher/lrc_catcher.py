import requests
import json
from bs4 import BeautifulSoup as bs
import re
import time
import random
import os
import lrc_catcher_config
import codecs
import sys

def lyric(song_id, list_name, song_name):
#    if os.path.exists('D:\\歌单\\'+list_name+'\\'+song_name+'.lrc'):
#        print('WARNING:Song %s already exists.' % song_name)
#        return
    url = 'http://music.163.com/api/song/media?id='+song_id# 436514312'
    try:
        r = requests.get(url)
        lrc = json.loads(r.text)['lyric']
        with codecs.open('D:\\歌单\\'+list_name+'\\'+song_name+'.lrc', 'w', 'utf8') as w:
            w.write(lrc)
        print('INFO:   Secceed in downloading lyric for song %s' % song_name)
        slp_t = random.random()*5+5
        print('INFO:   Now we sleep for %2fs\n' % slp_t)
        time.sleep(slp_t)
    except:
        print('WARNING:We got a failure on song %s, whose id is %s and bolong to list %s' % (song_name, song_id, list_name))
    
def find_song(list_id, list_name):
    headers = {
            'Referer':'http://music.163.com',
            'Host':'music.163.com',
            'User-Agent':'Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.132 Safari/537.36',
            }
    play_url = 'http://music.163.com/playlist?id='+list_id # 317113395'
    #　play_url = 'https://music.163.com/search/m/?s=成都 赵雷&type=1'
    s = requests.session()
    r = s.get(play_url, headers=headers).content
    s = bs(r,"lxml")
    main = s.find('ul', {'class':'f-hide'})
    for music in main.find_all('a'):
        song_id = re.match(r'.+?(\d+)', music['href']).groups(1)[0]
        try:
            lyric(song_id, list_name, music.text)
        except:
            print("ERROR:  Can't download lyric for %s\n" % music.text)

list_ids = lrc_catcher_config.list_ids
list_names = lrc_catcher_config.list_names

if sys.argv[1] == '-a':
    for i in range(len(list_ids)):
        try:
            find_song(list_ids[i], list_names[i])
        except:
            print("FATAL:  Can't get songs in list %s" % list_names[i])
        slp_t = random.random()*50+50
        print('INFO:   Secceed in downloading lyrics in list %s'% list_names[i])
        print('INFO:   Now we sleep for %2fs\n' % slp_t)
        time.sleep(slp_t)
elif sys.argv[1] == '-s':
    lyric(sys.argv[2], sys.argv[3], sys.argv[4])
