import random
import json
import pandas as pd

def generateRandom():
	r1 = random.randint(0, 255)
	if r1%10==0:
		return ".*.*.*"
	else:
		r2 = random.randint(0, 255)
		if r2%10==0:
			return "."+str(r1)+".*.*"
		else:
			r3 = random.randint(0, 255)
			if r3%10==0:
				return "."+str(r1)+"."+str(r2)+".*"
			else:
				return "."+str(r1)+"."+str(r2)+"."+str(r3)


def generateIP1A():
	return {"sourceIP":str(random.randint(0, 100))+generateRandom(),"destIP":str(random.randint(128, 255))+generateRandom(),"action":"allow"} 

def generateIP1B():
	return {"sourceIP":str(random.randint(60, 180))+generateRandom(),"destIP":str(random.randint(128, 200))+generateRandom(),"action":"block"} 

def generateIP2A():
	return {"sourceIP":str(random.randint(128, 255))+generateRandom(),"destIP":str(random.randint(128, 255))+generateRandom(),"action":"allow"} 

def generateIP2B():
	return {"sourceIP":str(random.randint(140, 240))+generateRandom(),"destIP":str(random.randint(140, 240))+generateRandom(),"action":"block"} 

def generateIP3A():
	return {"sourceIP":str(random.randint(0, 64))+generateRandom(),"destIP":str(random.randint(1, 128))+generateRandom(),"action":"allow"} 

def generateIP3B():
	return {"sourceIP":str(random.randint(65, 128))+generateRandom(),"destIP":str(random.randint(1, 128))+generateRandom(),"action":"block"} 

def generateIP4A():
	return {"sourceIP":str(random.randint(128, 255))+generateRandom(),"destIP":str(random.randint(40, 120))+generateRandom(),"action":"allow"} 

def generateIP4B():
	return {"sourceIP":str(random.randint(140, 240))+generateRandom(),"destIP":str(random.randint(1, 128))+generateRandom(),"action":"block"} 

def generateRules(num):
	result = []
	for j in range(0,num/8):
		result.append(generateIP1A())
	for j in range(0,num/8):
		result.append(generateIP1B())

	for j in range(0,num/8):
		result.append(generateIP2A())
	for j in range(0,num/8):
		result.append(generateIP2B())

	for j in range(0,num/8):
		result.append(generateIP3A())
	for j in range(0,num/8):
		result.append(generateIP3B())

	for j in range(0,num/8):
		result.append(generateIP4A())
	for j in range(0,num/8):
		result.append(generateIP4B())

	df = pd.DataFrame(result)
	df.to_csv('test.csv')

	# with open('rules.json', 'w') as outfile:  
	# 	json.dump(result, outfile)

generateRules(100)









