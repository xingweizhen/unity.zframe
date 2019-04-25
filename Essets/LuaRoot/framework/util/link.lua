-- File Name : util/link.lua

local ipairs, pairs, table
    = ipairs, pairs, table

local OBJDEF = { }
OBJDEF.__index = OBJDEF
OBJDEF.__tostring = function (self)
	return "[Link: ]"
end

function OBJDEF.new(data)
   	local self = { Data = data }
   	return setmetatable(self, OBJDEF)
end

function OBJDEF:append(data)
	local Next = self
	while Next.Next do
		Next = Next.Next
	end
	Next.Next = OBJDEF.new(data)
end

function OBJDEF:prepend(data)
	self.Prev = OBJDEF:new(data)
	self = self.Prev
end

function OBJDEF:clear()
	self.Next = nil
end

_G.DEF.Link = OBJDEF
