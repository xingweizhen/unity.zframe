--
-- @file    util/date.lua
-- @author  xing weizhen (kokohna@163.com)
-- @date    2016-01-17 11:22:10
-- @desc    日期工具
--

-- 格式				描述											示例
-- os.date("%a")	abbreviated weekday name 					Sun
-- os.date("%A")	full weekday name							Sunday
-- os.date("%b")	abbreviated month name 						Jul
-- os.date("%B")	full month name								July
-- os.date("%c")	date and time								07/27/14 17:42:09
-- os.date("%d")	day of the month							27
-- os.date("%H")	hour, using a 24-hour clock (23) [00-23]	17
-- os.date("%I")	hour, using a 12-hour clock (11) [01-12]	5
-- os.date("%M")	minute (48) [00-59]							42
-- os.date("%m")	month (09) [01-12]							7
-- os.date("%p")	either "am" or "pm" (pm)					PM
-- os.date("%S")	second (10) [00-61]							9
-- os.date("%w")	weekday (3) [0-6 = Sunday-Saturday]			0
-- os.date("%x")	date										07/27/14
-- os.date("%X")	time (e.g., 23:48:10)						17:42:09
-- os.date("%Y")	full year (1998)							2014
-- os.date("%y")	two-digit year (98) [00-99]					14

local libsystem = require "libsystem.cs"
local zero = os.time{year=1970, month=1, day=1, hour=8}
local origin = os.time{year=1970, month=12, day=31, hour=24}

local function date2secs(Date)
	return os.time(Date) - zero
end

local function secs2date(fmt, secs)
	if secs == nil then secs = date2secs() end
	return os.date(fmt, secs + zero)
end

local function secs2time(fmt, secs)
	if fmt == nil then fmt = "%H:%M:%S" end
	secs = origin + secs
	return os.date(fmt, secs)
end

-- 默认的秒数转时钟显示
local function secs2clock(secs)
	return secs2time(nil, secs)
end

-- 把持续秒数转化为可读性更强的持续时间
-- @ precision	精度
-- 		4 表示精确显示到秒; 3 分钟; 2 小时; 1 天
-- @ count 最多显示位数
local function last2string(lastSecs, precision, color, count)
	local TipTimeLast = _G.TEXT.TipTimeLast

	if precision == nil or precision == 0 then precision = 1 end
	if precision > 4 then precision = 4 end
	if count == nil or count == 0 then count = 4 end

	local Texts = {}
	local p = precision
	local c = count
	local t = 0
	local function make_text(secs, cycle, txt)
		if p > 0 then
			p = p - 1
			if secs >= cycle and c > t then
				local str = ""
				if color then
					str = string.format("[%s]%s[-]%s", color, math.floor(secs / cycle), txt)
				else
					str = math.floor(secs / cycle) .. txt
				end
				table.insert(Texts, str)
				secs = secs % cycle
				t = t + 1
			end
		end
		return secs
	end

	lastSecs = make_text(lastSecs, 86400, TipTimeLast.day)
	lastSecs = make_text(lastSecs, 3600, TipTimeLast.hour)
	lastSecs = make_text(lastSecs, 60, TipTimeLast.min)
	make_text(lastSecs, 1, TipTimeLast.sec)

	if #Texts > 0 then
		return table.concat(Texts, "")
	else
		local Array = { TipTimeLast.day, TipTimeLast.hour, TipTimeLast.min, TipTimeLast.sec }
		return TipTimeLast.fmtLessOneUnit:csfmt(Array[precision])
	end
end

-- 用来显示什么几分钟前 几天前
-- dateSecs时间戳（过去时间）或持续时间
-- isUnix是否是时间戳
local function secs2string_before(dateSecs, isUnix)
	local secs
	if isUnix then
		local nowSecs = date2secs()
		secs = nowSecs - (dateSecs or nowSecs)
	else
		secs = dateSecs
	end

	if secs < 0 then secs = 0 end

	local mins = math.floor(secs / 60)
	if mins < 1 then
		return TEXT.TimeAfter_recently
	elseif mins < 60 then
		return TEXT.TimeAfter_fmtMins:csfmt(mins)
	end

	local hours = math.floor(mins / 60)
	if hours < 24 then
		return TEXT.TimeAfter_fmtHours:csfmt(hours)
	end

	local days = math.floor(hours / 24)
	if days < 7 then
		return TEXT.TimeAfter_fmtDays:csfmt(days)
	end

	local weeks = math.floor(days / 7)
	if weeks < 52 then
		return TEXT.TimeAfter_fmtWeeks:csfmt(weeks)
	end

	local years = math.floor(weeks / 52)
	return TEXT.TimeAfter_fmtYears:csfmt(years)
end

-- 用来显示什么几分钟后 几天后
-- dateSecs时间戳（未来时间）或持续时间
-- isUnix是否是时间戳
local function secs2string_after(dateSecs, isUnix)
	local secs
	if isUnix then
		local nowSecs = date2secs()
		local secs = (dateSecs or nowSecs) - nowSecs
	else
		secs = dateSecs
	end

	if secs < 0 then secs = 0 end

	local mins = math.floor(secs / 60)
	if mins < 1 then
		return TEXT.TimeCntDown_fmtSecds:csfmt(secs)
	elseif mins < 60 then
		return TEXT.TimeCntDown_fmtMins:csfmt(mins)
	end

	local hours = math.floor(mins / 60)
	if hours < 24 then
		return TEXT.TimeCntDown_fmtHours:csfmt(hours)
	end

	local days = math.floor(hours / 24)
	if days < 7 then
		return TEXT.TimeCntDown_fmtDays:csfmt(days)
	end

	local weeks = math.floor(days / 7)
	if weeks < 52 then
		return TEXT.TimeCntDown_fmtWeeks:csfmt(weeks)
	end

	local years = math.floor(weeks / 52)
	return TEXT.TimeCntDown_fmtYears:csfmt(years)
end

-- 统一倒计时时间显示
-- dateSecs：时间戳
local function secs2string(dateSecs)
	local nowSecs = date2secs()
	if nowSecs < dateSecs then
		return secs2string_before(dateSecs, true)
	elseif nowSecs > dateSecs then
		return secs2string_after(dateSecs, true)
	else
		return TEXT.Time_Zero
	end
end

local function sync_time(serverTime)
	local offset = os.time() - serverTime
	libsystem.SyncServerTime(offset)
	zero = math.floor(offset + 0.5)
end

os.date2secs = date2secs
os.secs2date = secs2date
os.secs2time = secs2time
os.secs2clock = secs2clock
os.last2string = last2string
os.secs2string = secs2string
os.secs2string_after = secs2string_after
os.secs2string_before = secs2string_before

os.synctime = sync_time
