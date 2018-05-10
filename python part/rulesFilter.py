# encoding:utf-8
import pandas as pd
from IPy import IP

def judgeRuleSetA():
    df = pd.read_csv("RuleSetA.csv")
    result = []
    for i in range(0, len(df)):
        # 非单个IP地址情况
        if len(df['srcIP'][i].split('-')) == 2 and len(df['destIP'][i].split('-')) == 2:
            srcStartIP = IP(df['srcIP'][i].split('-')[0]).int()
            srcEndIP = IP(df['srcIP'][i].split('-')[1]).int()
            destStartIP = IP(df['destIP'][i].split('-')[0]).int()
            destEndIP = IP(df['destIP'][i].split('-')[1]).int()

            for j in range(i+1, len(df)):
                checkSrcStartIP = 0
                checkSrcEndIP = 0
                checkDestStartIP = 0
                checkDestEndIP = 0
                if len(df['srcIP'][j].split('-')) == 2 and len(df['destIP'][j].split('-')) == 2:
                    checkSrcStartIP=IP(df['srcIP'][j].split('-')[0]).int()
                    checkSrcEndIP=IP(df['srcIP'][j].split('-')[1]).int()
                    checkDestStartIP=IP(df['destIP'][j].split('-')[0]).int()
                    checkDestEndIP=IP(df['destIP'][j].split('-')[1]).int()
                    # 单个点的情况
                else:
                    checkSrcStartIP=IP(df['srcIP'][j].split('-')[0]).int()
                    checkSrcEndIP=IP(df['srcIP'][j].split('-')[0]).int()
                    checkDestStartIP=IP(df['destIP'][j].split('-')[0]).int()
                    checkDestEndIP=IP(df['destIP'][j].split('-')[0]).int()
                if checkSrcStartIP > srcStartIP and checkSrcEndIP < srcEndIP:
                    # print(checkStartIP)
                    if checkDestStartIP > destStartIP and checkDestEndIP < destEndIP:
                        if(df['action'][i] == df['action'][j]):
                            result.append(df['srcIP'][j] + " and " + df['destIP'][j])
    ipSet = set(result)
    print(len(result))
    print(len(ipSet))


def judgeRuleSetBInA():
    df1 = pd.read_csv("RuleSetA.csv")
    df2 = pd.read_csv("RuleSetB.csv")