--
-- @file    framework/u3dprofiler.lua
-- @author  xing weizhen (kokohna@163.com)
-- @date    2019-09-04 10:05:05
-- @desc    调用Unity的Profiler接口来分析Lua的函数调用
--			_G.PKG["framework/u3dprofiler"].start()	--启动	
--			_G.PKG["framework/u3dprofiler"].stop()	--停止
--

if _G.ENV.development then
	local libunity = require "libunity.cs"
	local sethook = xlua.sethook or debug.sethook
	local BeginSample, EndSample = libunity.BeginSample, libunity.EndSample
	local func_path_map = nil
	local SampleQueue = {}
	local ProfilerTags = {}

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

	local function start(tag)
		if tag == nil then tag = "global" end
		if ProfilerTags[tag] then return end

		if next(ProfilerTags) then
			ProfilerTags[tag] = true
			return
		end

		ProfilerTags[tag] = true
		func_path_map = {}
		sethook(on_hook, "cr", 0)
	end

	local function stop(tag)
		if tag == nil then tag = "global" end
		ProfilerTags[tag] = nil
		
		if not next(ProfilerTags) then
			for i=#SampleQueue,1,-1 do 
				SampleQueue[i] = nil
				EndSample() 
			end

		    sethook()
		    func_path_map = nil
		end
	end

	return {
		start = start,
		stop = stop,
	}
else
	local function empty( ... ) end
	return { start = empty, stop = empty }
end
