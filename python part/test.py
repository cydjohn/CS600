# encoding:utf-8

class rules(object):
	"""docstring for rules"""
	def __init__(self, srcIP,destIP,action):
		super(rules, self).__init__()
		self.srcIP = srcIP
		self.destIP = destIP
		self.action = action
		self.addIpaddress(self.srcIP,self.destIP,self.action)

	def addIpaddress(self,srcIP,destIP,action):
		self.srcIP = srcIP.split(',')
		self.destIP = destIP.split(',')
		self.action = action
		self.firstLayer = {'srcIP':self.srcIP[0],'destIP':self.destIP[0]}
		self.secondLayer = {'srcIP':self.srcIP[1],'destIP':self.destIP[1]}
		self.thirdLayer = {'srcIP':self.srcIP[2],'destIP':self.destIP[2]}
		self.forthLayer = {'srcIP':self.srcIP[3],'destIP':self.destIP[3]}

		
class node(object):
	"""docstring for node"""
	def __init__(self, srcIPStart,srcIPEnd,destIPStart,destIPEnd):
		super(node, self).__init__()
		self.srcIPStart = srcIPStart
		self.srcIPEnd = srcIPEnd
		self.destIPStart  = destIPStart
		self.destIPEnd = destIPEnd
		


class layer(object):
	"""docstring for node"""
	table = [[]]

	def __init__(self):
		super(layer, self).__init__()
		self.table = [[node(0,0,0,0) for i in range(255)] for i in range(255)]

	def insert(self,ipAddress):
		n = node(0,0,0,0)
		x = 0
		y = 0
		
				
		# 区分星号情况
		if ipAddress['srcIP'] == '*':
			n.srcIPStart = 0
			n.srcIPEnd = 255
		else:
			n.srcIPStart = int(ipAddress['srcIP'])
			n.srcIPEnd = int(ipAddress['srcIP'])
			x = n.srcIPStart


		if ipAddress['destIP'] == "*":
			n.destIPStart = 0
			n.destIPEnd = 255
		else:
			n.destIPStart = int(ipAddress['destIP'])
			n.destIPEnd = int(ipAddress['destIP'])
			y = n.destIPStart

		posX = 0
		posY = 0
		while self.table[posX][posY].srcIPStart<=n.srcIPStart and self.table[posX][posY].srcIPStart != 0:
			posX += 1

		while self.table[posX][posY].destIPStart<=n.destIPStart and self.table[posX][posY].destIPStart != 0:
			posY += 1

		print(posX," ",posY)

		self.table[posX][posY] = n


class TDTree(object):
	"""docstring for TDTree"""
	firstLayer = layer()
	secondLayer = layer()
	thirdLayer = layer()
	forthLayer = layer()

	def __init__(self):
		super(TDTree, self).__init__()
	
	def insertRules(self,rules):
		self.firstLayer.insert(rules.firstLayer)
		self.secondLayer.insert(rules.secondLayer)
		self.thirdLayer.insert(rules.thirdLayer)
		self.forthLayer.insert(rules.forthLayer)
	

		
		

if __name__ == '__main__':
	r = rules('1,2,3,*','2,2,*,*','allow')
	r2 = rules('2,3,3,*','2,4,*,*','allow')
	t = TDTree()
	t.insertRules(r)
	t.insertRules(r2)

	print t.firstLayer.table[0][0].srcIPEnd
	print t.secondLayer.table[0][0].srcIPEnd
	print t.thirdLayer.table[0][0].srcIPEnd
	print t.forthLayer.table[0][0].srcIPEnd



	

