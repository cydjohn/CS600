import pandas as pd
from IPy import IP




def translateToCSV(fileName):
	rSrc = []
	rDest = []
	df = pd.read_csv(fileName+".txt")
	for i in range(0,len(df)):
		srcIP = df["srcIP"][i]
		destIP = df["destIP"][i]
		rSrc.append(IP(srcIP,make_net=1).strNormal(3))
		rDest.append(IP(destIP,make_net=1).strNormal(3))

	result = pd.DataFrame({"srcIP":rSrc,"destIP":rDest,"action":df["action"]})
	result.to_csv(fileName+".csv")


translateToCSV("RuleSetA")
translateToCSV("RuleSetB")