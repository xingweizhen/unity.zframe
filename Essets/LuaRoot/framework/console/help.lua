--
-- @file 	framework/console/help.lua
-- @author  xing weizhen (kokohna@163.com)
-- @date	2016-05-12 14:56:32
-- @desc    描述
--

local function list(t, n)
	local List = { "!help" }
    for k,v in pairs(t) do
    	if k:sub(1, 2) ~= "__" then
    		local vType = type(v)
    		table.insert(List, 1, string.format("%s\t%s", vType, k))
        end
    end
    table.sort(List)
    print(table.concat(List, "\n"))
end

return { __index = list }

