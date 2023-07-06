#!/usr/bin/env python

# monitor.py
# 2016-09-17
# Public Domain

# monitor.py          # monitor all GPIO
# monitor.py 23 24 25 # monitor GPIO 23, 24, and 25

import os
import sys
import time
import uuid
import pigpio
import os.path
import logging
import datetime
import requests
import threading
from logging.handlers import TimedRotatingFileHandler

if not os.path.exists("logs"):
    os.makedirs("logs")
# create logger
logger = logging.getLogger("LOGGER")
logger.setLevel(logging.DEBUG)
handler = TimedRotatingFileHandler("logs/log_{}.txt".format(
    datetime.datetime.now().strftime('%Y-%m-%d')), when="midnight", interval=1)
formatter = logging.Formatter("%(asctime)s\t[%(levelname)s]\t%(message)s")
handler.setFormatter(formatter)
# finally add handler to logger
logger.addHandler(handler)

pigpio.exceptions = True

cb = []


class RequestThread(threading.Thread):
    def __init__(self, id, url, data, headers):
        super(RequestThread, self).__init__()
        self.id = id
        self.url = url
        self.data = data
        self.headers = headers

    def run(self):
        try:
            response = requests.post(
                url=self.url, json=self.data, headers=self.headers, timeout=5)
            logger.info("id{}\tresponse={} {}".format(
                self.id, response.status_code, response.reason))
        except Exception as error:
            logger.error(str(error))


def cbf(GPIO, level, tick):
    id = str(uuid.uuid4())
    url = "http://192.168.9.125/avc/service-center/update"
    payload = {"port": GPIO, "value": level}
    headers = {'Content-type': 'application/json', 'Accept': 'text/plain'}
    logger.info("id{}\tpayload={}".format(id, payload))
    thread = RequestThread(id, url, payload, headers)
    thread.start()


pi = pigpio.pi()
if not pi.connected:
    exit()

if len(sys.argv) == 1:
    G = range(2, 27)
else:
    G = []
    for a in sys.argv[1:]:
        G.append(int(a))

for g in G:
    pi.set_mode(g, pigpio.INPUT)
    pi.set_glitch_filter(g, 300000)
    pi.set_pull_up_down(g, pigpio.PUD_UP)
    cb.append(pi.callback(g, pigpio.EITHER_EDGE, cbf))

try:
    while True:
        time.sleep(60)
except KeyboardInterrupt:
    for c in cb:
        c.cancel()

pi.stop()
