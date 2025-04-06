import random
import simpy
import json
import os
from dotenv import load_dotenv

# .env 파일 로드
load_dotenv()

# .env에서 경로 가져오기
OUTPUT_PATH = os.getenv('OUTPUT_PATH')
if OUTPUT_PATH is None:
    raise ValueError("OUTPUT_PATH not defined in .env")

# 디렉터리 생성
output_dir = os.path.dirname(os.path.abspath(OUTPUT_PATH))
os.makedirs(output_dir, exist_ok=True)

# 시뮬레이션 설정
RANDOM_SEED = 42
NEW_CUSTOMERS = 5
INTERVAL_CUSTOMERS = 10.0
MIN_PATIENCE = 1
MAX_PATIENCE = 3

log = []

def source(env, number, interval, counter):
    for i in range(number):
        c = customer(env, f'Customer{i:02d}', counter, time_in_bank=12.0)
        env.process(c)
        t = random.expovariate(1.0 / interval)
        yield env.timeout(t)

def customer(env, name, counter, time_in_bank):
    arrive = env.now
    log.append({"time": round(arrive, 4), "event": "arrival", "customer": name})

    with counter.request() as req:
        patience = random.uniform(MIN_PATIENCE, MAX_PATIENCE)
        results = yield req | env.timeout(patience)

        wait = env.now - arrive

        if req in results:
            log.append({"time": round(env.now, 4), "event": "service_start", "customer": name})
            tib = random.expovariate(1.0 / time_in_bank)
            yield env.timeout(tib)
            log.append({"time": round(env.now, 4), "event": "service_end", "customer": name})
        else:
            log.append({"time": round(env.now, 4), "event": "renege", "customer": name})

print('Bank renege simulation with JSON logging')
random.seed(RANDOM_SEED)
env = simpy.Environment()
counter = simpy.Resource(env, capacity=1)
env.process(source(env, NEW_CUSTOMERS, INTERVAL_CUSTOMERS, counter))
env.run()

# 로그 저장
with open(OUTPUT_PATH, "w") as f:
    json.dump(log, f, indent=2)

print(f"Saved bank_log.json to {OUTPUT_PATH}")