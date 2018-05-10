# encoding:utf-8
import pandas as pd
from IPy import IP

def judgeRuleSetBInA(df1,df2):
    result = []
    for i in range(0, len(df1)):
        # 非单个IP地址情况
        if len(df1['srcIP'][i].split('-')) == 2 and len(df1['destIP'][i].split('-')) == 2:
            srcStartIP = IP(df1['srcIP'][i].split('-')[0]).int()
            srcEndIP = IP(df1['srcIP'][i].split('-')[1]).int()
            destStartIP = IP(df1['destIP'][i].split('-')[0]).int()
            destEndIP = IP(df1['destIP'][i].split('-')[1]).int()
            for j in range(i, len(df2)):
                checkSrcStartIP = 0
                checkSrcEndIP = 0
                checkDestStartIP = 0
                checkDestEndIP = 0
                if len(df2['srcIP'][j].split('-')) == 2 and len(df2['destIP'][j].split('-')) == 2:
                    checkSrcStartIP=IP(df2['srcIP'][j].split('-')[0]).int()
                    checkSrcEndIP=IP(df2['srcIP'][j].split('-')[1]).int()
                    checkDestStartIP=IP(df2['destIP'][j].split('-')[0]).int()
                    checkDestEndIP=IP(df2['destIP'][j].split('-')[1]).int()
                    # 单个点的情况
                else:
                    checkSrcStartIP=IP(df2['srcIP'][j].split('-')[0]).int()
                    checkSrcEndIP=IP(df2['srcIP'][j].split('-')[0]).int()
                    checkDestStartIP=IP(df2['destIP'][j].split('-')[0]).int()
                    checkDestEndIP=IP(df2['destIP'][j].split('-')[0]).int()
                if checkSrcStartIP > srcStartIP and checkSrcEndIP < srcEndIP:
                    # print(checkStartIP)
                    if checkDestStartIP > destStartIP and checkDestEndIP < destEndIP:
                        if(df2['action'][i] == df2['action'][j]):
                            result.append(df2['srcIP'][j] + " and " + df2['destIP'][j])
    ipSet = set(result)
    print(len(result))
    print(len(ipSet))


df1 = pd.read_csv("RuleSetA.csv")
df2 = pd.read_csv("RuleSetB.csv")
judgeRuleSetBInA(df1,df1)