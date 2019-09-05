--
-- @file    framework/util/math.lua
-- @author  xing weizhen (kokohna@163.com)
-- @date    2018-07-04 18:27:36
-- @desc    描述
--

function math.clamp(value, min, max)
	if value < min then return min end
	if value > max then return max end

	return value
end

function math.round(value)
	return math.floor(value + 0.5)
end
