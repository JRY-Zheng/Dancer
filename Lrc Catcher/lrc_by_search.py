import requests
import json
from bs4 import BeautifulSoup as bs
import re


def baidu_music_id(singer, song_name):
    url = 'http://music.baidu.com/search/lrc?key='
    url += singer + '%20' + song_name
    r = requests.session().get(url)
    s = bs(r.content, 'html5lib')
    title = s.find('span', {'class':'song-title'})
    h = title.find('a')['href']
    return re.match(r"/song/(.*)", h).groups(1)[0]
