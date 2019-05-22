--
-- @file    framework/clock.lua
-- @author  xing weizhen (kokohna@163.com)
-- @date    2016-02-29 11:11:58
-- @desc    描述
--

local DY_TIMER = _G.DY_TIMER

local P = {}

function P.update_clock(pass)
	local Timers = DY_TIMER.Timers
    for i=#Timers,1,-1 do
        local Tm = Timers[i]
        trycall(Tm.update, Tm, pass)
        -- Tm:update(pass)
        if Tm.paused then
            table.remove(Timers, i)
            trycall(Tm.stop, Tm)
        end
    end
end

local TempActions = setmetatable({}, _G.MT.AutoGen)
local FrameActions = {}
function P.update_frame()
    local libunity = libunity
    for k,Actions in pairs(FrameActions) do
        if libunity.IsActive(k, true, true) then
            for _,action in ipairs(Actions) do
                trycall(action)
            end
        end
        FrameActions[k] = nil
    end
    for k,Actions in pairs(TempActions) do
        FrameActions[k] = Actions
        TempActions[k] = nil
    end
end

function P.add_action(key, action)
    if action == nil then
        libunity.LogE("\"add_action\"失败，方法为空！")
    end
    table.insert_once(TempActions[key], action)
end

function P.remove_action(key, action)
    if action == nil then
        FrameActions[key] = nil
        TempActions[key] = nil
    else
        table.remove_elm(FrameActions[key], action)
        table.remove_elm(TempActions[key], action)
    end
end

return P
