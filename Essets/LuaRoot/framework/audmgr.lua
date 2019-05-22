--
-- @file    framework/audmgr.lua
-- @author  xingweizhen (weizhen.xing@funplus.com)
-- @date    2018-06-02 15:06:46
-- @desc    描述
--

local BGMStack = _G.DEF.Stack.new()

local P = {}

local function stop_bgm_event(event)
	if event and #event > 0 then
		libunity.StopAudio(event, P.parent)
	end
end

local function play_bgm_event(event)
	if event and #event > 0 then
		libunity.PlayAudio(event, P.parent, true)
	end
end

local function switch_event(currEvent, nextEvent)
	if currEvent ~= nextEvent then
		stop_bgm_event(currEvent)
	end
	play_bgm_event(nextEvent)
end

function P.push(nextEvent)
	switch_event(BGMStack:peek(), BGMStack:push(nextEvent))
end

function P.pop()
	switch_event(BGMStack:pop(), BGMStack:peek())
end

-- 清空栈，并播放一个新的背景压栈
function P.new(nextEvent)
	local currEvent = BGMStack:pop()
	BGMStack:clear()
	switch_event(currEvent, BGMStack:push(nextEvent))
end

_G.AUD = P
