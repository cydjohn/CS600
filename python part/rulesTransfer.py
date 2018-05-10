import pandas as pd
from IPy import IP




def translateToCSV(fileName):
	rSrc = []
	rDest = []
	srcStart = []
	srcEnd = []
	destStart = []
	destEnd = []
	df = pd.read_csv(fileName+".txt")
	for i in range(0,len(df)):
		srcIP = df["srcIP"][i]
		destIP = df["destIP"][i]
		rSrc.append(IP(srcIP,make_net=1).strNormal(3))
		rDest.append(IP(destIP,make_net=1).strNormal(3))

		srcTemp = IP(srcIP,make_net=1).strNormal(3).split('-')
		if len(srcTemp) == 2:
			srcStart.append(srcTemp[0])
			srcEnd.append(srcTemp[1])
		else:
			srcStart.append(srcTemp[0])
			srcEnd.append(srcTemp[0])

		destTemp = IP(destIP,make_net=1).strNormal(3).split('-')
		if len(destTemp) == 2:
			destStart.append(destTemp[0])
			destEnd.append(destTemp[1])
		else:
			destStart.append(destTemp[0])
			destEnd.append(destTemp[0])


	result = pd.DataFrame({"srcIP":rSrc,"destIP":rDest,"action":df["action"]})
	result2 = pd.DataFrame({"srcIPStart":srcStart,"srcIPEnd":srcEnd,"destIPStart":destStart,"destIPEnd":destEnd,"action":df["action"]})
	
	result.to_csv(fileName+".csv")
	result2.to_csv(fileName+"Plus.csv")


translateToCSV("RuleSetA")
translateToCSV("RuleSetB")