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

local Command
local mtHelp = dofile("framework/console/help")
do
    local DataDEF = dofile("framework/console/datatool")
    Command = {
        lua = function (_, ...)
            local code = table.concat({...}, " ")
            local func = load(code)
            if func then
                local status, ret = trycall(func)
                if status then return ret end
            else
                libunity.LogE("lua load fail!: {0}", code)
            end
        end,
        ver = function () 
            local AppVer, AssetVer = libasset.GetVersion()
            print("App  ", table.line(AppVer))
            print("Asset", table.line(AssetVer))
        end,

        ui = setmetatable(dofile("framework/console/uitool"), mtHelp),
        net = setmetatable(dofile("framework/console/nettool"), mtHelp),
        
        -- dydata = setmetatable({ name = "DYDATA", Data = _G.PKG["datamgr/dydata"] }, DataDEF),
        -- uidata = setmetatable({ name = "UIDATA", Data = _G.PKG["datamgr/uidata"] }, DataDEF),        
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

local function add_tool(name, path)
    local cmd = dofile(path)
    local cmdType = type(cmd)
    if cmdType == "table" then
        Command[name] = setmetatable(cmd, mtHelp)
    elseif cmdType == "function" then
        Command[name] = cmd
    end
end

local function add_cmd(action, ...)
    local Args = {...}
    local Module, cmd = Command, Args[#Args]
    if Module then
        for i=1,#Args-1 do
            local key = Args[i]
            local M = rawget(Module, key)
            if M == nil then
                M = setmetatable({}, mtHelp)
                Module[key] = M
            end
            Module = M
        end
        Module[cmd] = action
    else
    end
end

return {
    add_tool = add_tool,
    add_cmd = add_cmd,
    parse_cmd = parse_cmd,
}