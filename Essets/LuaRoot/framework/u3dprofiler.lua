--
-- @file    framework/u3dprofiler.lua
-- @author  xing weizhen (kokohna@163.com)
-- @date    2019-09-04 10:05:05
-- @desc    调用Unity的Profiler接口来分析Lua的函数调用
-- 			require(framework/u3dprofiler).start()	--启动	
--			require(framework/u3dprofiler).stop() 	--停止
--

local libunity = require "libunity.cs"
local sethook = xlua.sethook or debug.sethook
local BeginSample, EndSample = libunity.BeginSample, libunity.EndSample
local func_path_map = nil
local SampleQueue = {}

local function on_hook(event, func_info_id, source)		
	local path = func_path_map[func_info_id]

	if event == "call" then
		if path == nil then
			local Inf = debug.getinfo(2, "nS")
			local funcName, line = Inf.name, Inf.linedefined
			if funcName == nil then funcName = "[anonymous]" end
			local short_src = Inf.short_src
			short_src = short_src:match('"(.*)"') or short_src
			short_src = short_src:gsub("/", ".")

			path = short_src .. "." .. funcName .. "@" .. line
			func_path_map[func_info_id] = path
		end

		SampleQueue[#SampleQueue + 1] = path
		--print(string.rep("--", #SampleQueue) .. path .. " call")
		BeginSample(path)
	elseif event == "return" and path then
		local sampleDepth = #SampleQueue
		if SampleQueue[sampleDepth] == path then
			--print(string.rep("--", sampleDepth) .. path .. " return")			
			EndSample()
			SampleQueue[sampleDepth] = nil
		end
	end
end

local function start()
	func_path_map = {}
	calldepth = 0
	sethook(on_hook, "cr", 0)
end

local function stop()
    sethook()
    func_path_map = nil
end

return {
	start = start,
	stop = stop,
}
