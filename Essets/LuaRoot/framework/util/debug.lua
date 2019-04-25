local LogType ={"I", "D", "W", "E"}
local DefaultLogType = LogType[2] --"D"

local function cprint(log, color, showTrace, logType)
	local logStr
	if type(log) == "table" then
		logStr = "{\n"
		for key,value in pairs(log) do
			logStr = logStr .."\t"..tostring(key)..":"..tostring(value).."\n"
		end
		logStr = logStr .. "}"
		--logStr = cjson.encode(log, true)
	else
		logStr = tostring(log)
	end

	local trace = ""
	if showTrace then
		trace = debug.traceback(nil , 3)
	end

	if logType == nil then
		logType = DefaultLogType
	elseif type(logType) == "number" then
		logType = LogType[logType]
	end

	if color == nil then
		libunity.Log(2, logType, logStr)
	else
		local fmt = "<color={0}>{1}</color>\n{2}"
		libunity.Log(2, logType, fmt:csfmt(color, logStr, trace))
	end
end

function debug.printR(log, showTrace, logType)
	cprint(log, "red", showTrace, logType)
end

function debug.printY(log, showTrace, logType)
	cprint(log, "yellow", showTrace, logType)
end

function debug.printG(log, showTrace, logType)
	cprint(log, "green", showTrace, logType)
end

function debug.print(log, showTrace, logType)
	cprint(log, nil, showTrace, logType)
end

function debug.LogE(log)
	cprint(log, "red", true, "E")
end