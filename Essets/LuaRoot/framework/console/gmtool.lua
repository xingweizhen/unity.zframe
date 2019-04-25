--
-- @file    framework/console/gmtool.lua
-- @anthor  xing weizhen (xingweizhen@rongygame.com)
-- @date    2016-02-29 11:11:58
-- @desc    描述
--

local Presets = {
    ["1"] = [[
item add 1002 1
item add 1003 1
item add 1005 1
item add 1006 1
item add 1007 1
item add 2009 1
item add 2010 1
item add 2011 1
item add 2012 1
item add 2014 1
item add 2016 1
item add 2017 1
item add 2018 1
item add 2019 1
]],
}

local function sc_do_gm_cmd(cmdline)
   local NW = _G.PKG["network/networkmgr"]
    if NW.connected() then
        local nm = NW.msg("COM.CS.EXEC_GM_CMD")
        nm:writeString(cmdline)
        NW.send(nm)
    else
        _G.UI.Toast.make(nil, "无网络连接"):show()
    end
end

local function send_gm_command(P, ...)
    local Args = {...}
    if #Args > 0 then
        local cmdline
        if Args[1] == "!" then
            cmdline = Presets[Args[2]]
        else
            cmdline = table.concat(Args, " ")
        end
        if cmdline then
            sc_do_gm_cmd(cmdline)
        end
    end
end

NW.regist("COM.SC.EXEC_GM_CMD", function (nm)
    local ret = nm:readU32()
    if ret ~= 1 then
        local str = nm:readString()
        print("\n" .. str)
    end
end)

return send_gm_command
