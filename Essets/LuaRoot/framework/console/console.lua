--
-- @file    framework/console/console.lua
-- @author  xing weizhen (kokohna@163.com)
-- @date    2016-02-29 11:11:58
-- @desc    描述
--

local tonumber, table
    = tonumber, table

local libunity = require "libunity.cs"
local libugui = require "libugui.cs"
local libasset = require "libasset.cs"

local goConsole

local Command
do
    local mtHelp = dofile("framework/console/help")
    local DataDEF = dofile("framework/console/datatool")
    Command = {
        lua = function (_, ...)
            local code = table.concat({...}, " ")
            local func = load(code)
            if func then
                local status, ret = trycall(func)
                if status then return ret end
            end

            libunity.LogE("lua load fail!: {0}", code)
        end,
        -- gm = dofile("framework/console/gmtool"),
        ui = setmetatable(dofile("framework/console/uitool"), mtHelp),
        net = setmetatable(dofile("framework/console/nettool"), mtHelp),
        battle = setmetatable(dofile("framework/console/battletool"), mtHelp),

        -- dydata = setmetatable({ name = "DYDATA", Data = _G.PKG["datamgr/dydata"] }, DataDEF),
        -- uidata = setmetatable({ name = "UIDATA", Data = _G.PKG["datamgr/uidata"] }, DataDEF),
        -- cfg = setmetatable({ name = "CFG", Data = _G.CFG }, DataDEF),
        -- cfg = _G.PKG["console/config.lua"),
        -- dy = _G.PKG["console/dydata.lua"),
        -- ver = {
        --     show = function () print(libasset.GetVersion()) end,
        -- }
    }
    setmetatable(Command, mtHelp)
end


local function help(Cmd)
    local Ret = {}
    for k,_ in pairs(Cmd) do
        table.insert(Ret, k)
    end
    return table.concat(Ret, "\n")
end

local function exec_cmd(CMD, key, ...)
    local Cmd = CMD[key]
    local cmdType = type(Cmd)
    if cmdType == "function" then
        return Cmd(CMD, ...)
    elseif cmdType == "table" then
        return exec_cmd(Cmd, ...)
    end
end

local function parse_cmd(cmdline)
    local Param = cmdline:split(" ")
    if #Param > 0 then
        return exec_cmd(Command, table.unpack(Param))
    end
end

-- local function set_output_cbf(output_cbf)
--     output = output_cbf
-- end

-- local function show_output(text)
--     if output then
--         output(text)
--     end
-- end

local function open_console()
    local Wnd = ui.find("FRMConsole")
    if Wnd == nil then
        Wnd = ui.show("UI/FRMConsole", 110)
        local input = libunity.Find(Wnd.go, "inpCmd")
        libugui.Select(input, true)
    else
        Wnd:close()
    end
end

return {
    parse_cmd = parse_cmd,
    -- set_output_cbf = set_output_cbf,
    -- show_output = show_output,
    open_console = open_console,
}