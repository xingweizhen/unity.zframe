--
-- @file    framework/util/class.lua
-- @anthor  xing weizhen (xingweizhen@rongygame.com)
-- @date    2016-01-14 13:28:08
-- @desc    类管理
--

 -- lua类继承c#类
function class(Sub, Base)
    local BaseMT = getmetatable(Base)
    Sub.__index = BaseMT.__index
    Sub.__newindex = BaseMT.__newindex
    if rawget(Sub, "__tostring") == nil then
    	Sub.__tostring = BaseMT.__tostring
    end
    setmetatable(Sub, BaseMT)
    return Sub
end
