--
-- @file    datamgr/dytimer.lua
-- @author  xing weizhen (kokohna@163.com)
-- @date    2016-11-30 15:48:39
-- @desc    描述
--

local libunity = require "libunity.cs"

-- 定时器类
local OBJDEF = {}
OBJDEF.__index = OBJDEF
OBJDEF.__tostring = function (self)
    if self then
        return string.format("[定时器:%s,周期=%d,倒数=%d,暂停=%s]",
            self.tag, self.cycle, self.count, tostring(self.paused))
    else
        return "[TimerDEF]"
    end
end

function OBJDEF.new(tag, count, cycle, on_cycle)
    if count < 0 then count = 0 end

    local self = {
        tag = tag,
        param = nil,
        count = count,
        cycle = cycle,
        onCycle = on_cycle,
        paused = false,
        CycleSet = {},
        CountingSet = {},
    }
    return setmetatable(self, OBJDEF)
end

function OBJDEF:init(count, cycle, cbf)
    if count < 0 then count = 0 end
    if count then self.count = count end
    if cycle then self.cycle = cycle end
    if cbf then self.onCycle = cbf end
    self.paused = count == 0
end

function OBJDEF:subscribe_cycle(key, on_cycle)
    self.CycleSet[key] = on_cycle
end

function OBJDEF:subscribe_counting(key, on_counting)
    self.CountingSet[key] = on_counting
end

function OBJDEF:update(n)
    if n == nil then n = 1 end
    if not self.paused then
        local counting = self.count
        local count = counting
        while n > 0 and not self.paused do
            if n > counting then
                n = n - counting
                counting = 0
            else
                counting = counting - n
                n = 0
            end
            count = counting
            if counting == 0 then
                -- 周期处理
                if self.onCycle then
                    self.paused = self:onCycle()
                else
                    self.paused = true
                end
                -- 周期发布
                for o,on_cycle in pairs(self.CycleSet) do
                    if libunity.IsActive(o, true, true) then
                        on_cycle(self)
                    else
                        self.CycleSet[o] = nil
                    end
                end
                if not self.paused then
                    counting = self.cycle
                end
            end
        end
        self.count = count

        -- 计数发布
        for o,on_counting in pairs(self.CountingSet) do
            if libunity.IsActive(o, true, true) then
                on_counting(self)
            else
                self.CountingSet[o] = nil
            end
        end
        self.count = counting
    end
end

function OBJDEF:stop()
    print("stopped", self)
    if self.onStopped then self:onStopped() end
end

function OBJDEF:to_time_string(fmt)
    local day = 0
    local seconds = self.paused and 0 or self.count
    if seconds > 86400 then
        day = math.floor(seconds / 86400)
        seconds = seconds % 86400
    end

    if fmt == nil then
        if seconds < 3600 then
            fmt = "%M:%S"
        else
            fmt = "%H:%M:%S"
        end
    end
    if day == 0 then
        fmt = fmt:gsub("%%d[^%%]+(.*)", "%1")
    else
        fmt = fmt:gsub("%%d", day)
    end
    return os.secs2time(fmt, seconds)
end

function OBJDEF:to_time_string_hour()
    local seconds = self.paused and 0 or self.count

    local h = math.floor(seconds / 3600)
    seconds = seconds - h * 3600
    local m = math.floor(seconds / 60)
    seconds = seconds - m * 60
    local s = math.floor(seconds)

    if h <= 0 then
        return string.format("%02d:%02d", m, s)
    else
        return string.format("%02d:%02d:%02d",h, m, s)
    end
end

-- 定时器管理类
local P = {
    Timers = {},
}

local function default_cycle(Tm) return true end

function P.get_timer(tag)
    for _,v in ipairs(P.Timers) do
        if v.tag == tag then return v end
    end
end

function P.new_timer(tag, count, cycle, cbf)
    if cbf == true then cbf = default_cycle end
    local tm = OBJDEF.new(tag, count, cycle, cbf)
    table.insert(P.Timers, tm)
    return tm
end

function P.replace_timer(tag, count, cycle, cbf)
    local tm = P.get_timer(tag)
    if tm == nil then
        tm = P.new_timer(tag, count, cycle, cbf)
    else
        tm:init(count, cycle, cbf)
    end
    return tm
end

function P.launch_timer(tag, count, cycle, cbf)
    local tm = P.get_timer(tag)
    if tm == nil then
        return P.new_timer(tag, count, cycle, cbf)
    else
        libunity.LogW("已存在相同tag({0})的定时器：{1}", tm)
    end
end

function P.stop_timer(tag)
    local tm = P.get_timer(tag)
    if tm then tm.paused = true end
end

function P.stop_timers(group)
    for _,v in ipairs(P.Timers) do
        if v.group == group then
            v.paused = true
        end
    end
end

function P.unsubscribe(go)
    for i,v in ipairs(P.Timers) do
        v.CycleSet[go] = nil
        v.CountingSet[go] = nil
    end
end

function P.clear()
    for i,v in ipairs(P.Timers) do 
        if not v.forever then
            v.paused = true 
        end
    end
end

return P
