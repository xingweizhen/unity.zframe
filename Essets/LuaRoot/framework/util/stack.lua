local type, ipairs, pairs, setmetatable, table
    = type, ipairs, pairs, setmetatable, table

local OBJDEF = { }
OBJDEF.__index = OBJDEF
OBJDEF.__tostring = function (self)
	local Texts = {}
	for _,v in ipairs(self) do
		table.insert(Texts, tostring(v))
	end
	return table.concat(Texts, '<-')
end

function OBJDEF.new()
   	local self = { }
   	return setmetatable(self, OBJDEF)
end

function OBJDEF:push(data)
	table.insert(self, 1, data)
	return data
end

function OBJDEF:pop()
	return table.remove(self, 1)
end

function OBJDEF:peek(n)
	if n == nil then n = 0 end
	return self[n + 1]
end

function OBJDEF:clear()
	while #self > 0 do
		table.remove(self)
	end
end

function OBJDEF:top()
	return #self
end

function OBJDEF:traversal(func)
	for _,v in ipairs(self) do
		if type(func) == "function" then
			func(v)
		end
	end
end

_G.DEF.Stack = OBJDEF
