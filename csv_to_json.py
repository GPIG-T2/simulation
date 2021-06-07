import json
from csv import reader
from math import sin, cos, sqrt, atan2, radians

# Gets distance between two latitudes/longtitudes
#https://stackoverflow.com/questions/19412462/getting-distance-between-two-points-based-on-latitude-longitude
def getDist(inLat1,inLon1,inLat2,inLon2):

    #radius of the earth in km
    R = 6373

    lat1 = radians(inLat1)
    lat2 = radians(inLat2)
    lon1 = radians(inLon1)
    lon2 = radians(inLon2)

    dlon = lon2 - lon1
    dlat = lat2 - lat1
    a = (sin(dlat/2))**2 + cos(lat1) * cos(lat2) * (sin(dlon/2))**2
    c = 2 * atan2(sqrt(a), sqrt(1-a))
    distance = R * c

    return int(distance)

#converts string lat/long to float
def latToFloat(inLat):
    if inLat[-1] == "W" or inLat[-1] == "S":
        return -float(inLat[:len(inLat)-1])
    else:
        return float(inLat[:len(inLat)-1])
#MAIN
worldDict = {}
mapNum = input("Enter world number: ")
inCSV = input("Enter csv: ")

worldDict["map"] = int(mapNum)
worldDict["virus"] = {
	"infectivity": {
		"underFive": 0.01,
		"fiveToSeventeen": 0.02,
		"eighteenToTwentyNine": 0.04,
		"thirtyToThirtyNine": 0.04,
		"fourtyToFourtyNine": 0.04,
		"fiftyToSixtyFour": 0.04,
		"sixtyFiveToSeventyFour": 0.02,
		"seventyFiveToEightyFour": 0.02,
		"overEightyFive": 0.04
	},
	"fatality": {
		"underFive": 0.0001,
		"fiveToSeventeen": 0.0001,
		"eighteenToTwentyNine": 0.001,
		"thirtyToThirtyNine": 0.0045,
		"fourtyToFourtyNine": 0.0130,
		"fiftyToSixtyFour": 0.044,
		"sixtyFiveToSeventyFour": 0.130,
		"seventyFiveToEightyFour": 0.320,
		"overEightyFive": 0.87
	},
	"reinfectivity": {
		"underFive": 0.001,
		"fiveToSeventeen": 0.001,
		"eighteenToTwentyNine": 0.001,
		"thirtyToThirtyNine": 0.001,
		"fourtyToFourtyNine": 0.001,
		"fiftyToSixtyFour": 0.001,
		"sixtyFiveToSeventyFour": 0.001,
		"seventyFiveToEightyFour": 0.001,
		"overEightyFive": 0.001
	},
	"symptomaticity": {
		"underFive": 0.5,
		"fiveToSeventeen": 0.5,
		"eighteenToTwentyNine": 0.5,
		"thirtyToThirtyNine": 0.5,
		"fourtyToFourtyNine": 0.5,
		"fiftyToSixtyFour": 0.5,
		"sixtyFiveToSeventyFour": 0.5,
		"seventyFiveToEightyFour": 0.5,
		"overEightyFive": 0.5
	},
	"seriousRate": {
		"underFive": 0.02,
		"fiveToSeventeen": 0.01,
		"eighteenToTwentyNine": 0.06,
		"thirtyToThirtyNine": 0.1,
		"fourtyToFourtyNine": 0.15,
		"fiftyToSixtyFour": 0.25,
		"sixtyFiveToSeventyFour": 0.4,
		"seventyFiveToEightyFour": 0.65,
		"overEightyFive": 0.95
	}
}

nodes = []
edgeData = []
globalPassengers = 0
with open(inCSV,'r') as read_obj:
    csv_reader = reader(read_obj)
    header = next(csv_reader)
    for row in csv_reader:
        node = {}
        node["population"] = int(row[2])
        node["interactivity"] = {
				"underFive": float(row[17]),
				"fiveToSeventeen": float(row[17]),
				"eighteenToTwentyNine": float(row[17]),
				"thirtyToThirtyNine": float(row[17]),
				"fourtyToFourtyNine": float(row[17]),
				"fiftyToSixtyFour": float(row[17]),
				"sixtyFiveToSeventyFour": float(row[17]),
				"seventyFiveToEightyFour": float(row[17]),
				"overEightyFive": float(row[17])
                                }
        node["name"] = row[1]
        node["position"] = {"x": 0, "y": 0}
        node["demographics"] ={
                                "underFive": float(row[3]),
				"fiveToSeventeen": float(row[4]),
				"eighteenToTwentyNine": float(row[5]),
				"thirtyToThirtyNine": float(row[6]),
				"fourtyToFourtyNine": float(row[7]),
				"fiftyToSixtyFour": float(row[8]),
				"sixtyFiveToSeventyFour": float(row[9]),
				"seventyFiveToEightyFour": float(row[10]),
				"overEightyFive": float(row[11])
                                }
        node["gdp"] = int(row[12])
        node["testingCapacity"] = int(row[13])
        edgeData.append((row[1],int(row[14]),latToFloat(row[15]),latToFloat(row[16])))
        globalPassengers += int(row[14])
        nodes.append(node)

worldDict["nodes"] = nodes

edges = []
for i,e in enumerate(edgeData):
    for j in range(i+1,len(edgeData)):
        if (int((e[1] * (edgeData[j][1]/globalPassengers))/365.25) != 0):
            edge = {}
            edge["name"] = e[0] + "<->" + edgeData[j][0]
            edge["left"] = i
            edge["right"] = j
            edge["population"] = int((e[1] * (edgeData[j][1]/globalPassengers))/365.25)
            edge["interactivity"] = 10
            edge["distance"] = getDist(e[1],e[2],edgeData[j][1],edgeData[j][2])
            edges.append(edge)

worldDict["edges"] = edges

jsonFile = input("Enter json file name: ")
jsonFile = "WorldFiles/" + jsonFile + ".json"
with open(jsonFile,"w") as write_obj:
    json.dump(worldDict, write_obj)




    
