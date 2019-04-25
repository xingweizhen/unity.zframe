--
-- @file    framework/util/string.lua
-- @anthor  xing weizhen (xingweizhen@rongygame.com)
-- @date    2016-02-29 11:11:58
-- @desc    描述
--

-- 模式匹配中Lua支持的所有字符类
    -- .   任意字符
    -- %a  字母
    -- %c  控制字符
    -- %d  数字
    -- %l  小写字母
    -- %p  标点字符
    -- %s  空白符
    -- %u  大写字母
    -- %w  字母和数字
    -- %x  十六进制数字
    -- %z  代表0的字符
-- 上面字符类的大写形式表示小写所代表的集合的补集比如：
    -- %A 非字母的字符
    -- ...
-- 模式匹配中的特殊字符：
    -- (    )   .   %   +   -   *   ?   [   ^   $
    -- 其中%作为特殊字符的转义符，还可以用于所有的非字母的字符的转义符
-- 可以用中括号定义自己的字符类，称作“字符集(char-set)”：
    -- [%w_]    字母、数字和下划线
    -- [01]     二进制数字（即0或1）
    -- [0-9]    即%d
-- 在字符集(char-set)的开始处使用 '^' 表示其补集：[^0-7]

-- 模式修饰符
    -- + 匹配前一字符 1 次或多次，总是进行最长匹配
    -- * 匹配前一字符 0 次或多次，总是进行最长匹配
    -- - 匹配前一字符 0 次或多次，总是进行最短匹配
    -- ? 匹配前一字符0次或1次
    -- 比如"%a+"匹配一个或多个字符（单词）；"%d+"匹配一个或多个数字（整数）
-- 以 '^' 开头的模式只匹配目标串的开始部分，以 '$' 结尾的模式只匹配目 标串的结尾部分

-- '%b' 用来匹配对称的字符。常写为 '%bxy' ，x 和 y 是任意两个不同的字符；
-- x 作为 匹配的开始,y 作为匹配的结束。
-- 比如,'%b()' 匹配以 '(' 开始,以 ')' 结束的字符串

-- 捕获(Captures)
-- Capture是这样一种机制:可以使用模式串的一部分匹配目标串的一部分。将你想
-- 捕获的模式用圆括号括起来,就指定了一个capture。

local StringFmt = libsystem.StringFmt

-- none break space
-- string.nbs = string.char(160)

function string.csfmt(format, ...)
    return StringFmt(format, ...)
end

function string:trim()
    return self:gsub("^%s*(.-)%s*$", "%1")
end

function string:split(p, nilIfEmpty)
    local insert = table.insert
    local fields = {}
    local pattern = string.format("[^%s]+", p)
    for w in self:gmatch(pattern) do insert(fields, w) end

    if p == "." then p = "%." end
    if (self:find(p)) == 1 then
        table.insert(fields, 1, "")
    end

    if nilIfEmpty and #fields == 0 then return nil end
    return fields
end

function string:splitn(p, nilIfEmpty)
    local insert, tonumber = table.insert, tonumber
    local fields = {}
    local pattern = string.format("[^%s]+", p)
    for w in self:gmatch(pattern) do insert(fields, tonumber(w)) end

    if nilIfEmpty and #fields == 0 then return nil end
    return fields
end

-- 根据"2001:1|1002:2"格式的字符串生成一个表
-- {
--     { id = 2001, amount = 1 ,},
--     { id = 1002, amount = 2 ,},
-- }
function string:splitgn(p)
    local insert = table.insert

    local Ret = {}
    local pattern = string.format("(%%d+)%s(%%d+)", p)
    for id, amount in self:gmatch(pattern) do
        insert(Ret, { id = tonumber(id), amount = tonumber(amount) } )
    end

    return Ret
end

function string:split_normal()
	local Ret = {}
	local datas = self:split('|')
	for _,v in pairs(datas) do
		local pairInfo = v:split(':')
		table.insert(Ret, { key = pairInfo[1], value = pairInfo[2] } )
	end

    return Ret
end

function string:totable(p, ...)
    local insert = table.insert

    local T = {}
    local Keys = { ... }
    local cap = string.format("([^%s]+)", p)
    local pattern = string.rep(cap, #Keys, p)
    local itor = self:gmatch(pattern)

    while true do
        local Values = { itor() }
        if #Values > 0 then
            for i,v in ipairs(Keys) do
                local value = Values[i]
                T[v] = tonumber(value) or value
            end
        else break end
    end

    return T
end

function string:totablearray(s, p, ...)
    local Array = {}
    local pattern = string.format("([^%s]+)", s)
    for value in self:gmatch(pattern) do
        table.insert(Array, value:totable(p, ...))
    end
    return Array
end

-- 获取目录名
function string:getdir()
    return self:match(".*/")
end

-- 获取文件名
function string:getfile()
    return self:match(".*/(.*)")
end

function string.tag(text, Tags)
    local k, v = next(Tags)
    if k and v then
        Tags[k] = nil
        v = tostring(v)
        if #v > 0 then
            return string.tag(string.format("<%s=%s>%s</%s>", k, v, text, k), Tags)
        else
            return string.tag(string.format("<%s>%s</%s>", k, text, k), Tags)
        end
    end

    return text
end

function string.color(text, c)
    return string.tag(text, { color = c, })
end

-- "需要数值"
function string.require(req, curr)
    if curr < req then
        return string.color(tostring(req), "#FF0000")
    else
        return tostring(req)
    end
end

-- "<拥有数量>/<需要数量>"
function string.own_needs(own, need, color, achieveColor)
    if own < need then
        return string.color(string.format("%d/%d", own, need), color or "#FF0000")
    else
        if achieveColor then
           return string.color(string.format("%d/%d", own, need), achieveColor)
        else
            return string.format("%d/%d", own, need)
        end
    end
end

function string.needs(own, need, color, achieveColor)
    if own < need then
        return string.color(tostring(need), color or "#FF0000")
    else
        if achieveColor then
           return string.color(tostring(need), achieveColor)
        else
            return string.format("%d", need)
        end
    end
end

function checknumber(value, base)
    return tonumber(value, base) or 0
end

function string.formatnumberthousands(num)
    local formatted = tostring(checknumber(num))
    local k
    while true do
        formatted, k = string.gsub(formatted, "^(-?%d+)(%d%d%d)", '%1.%2')
        if k == 0 then break end
    end
    return formatted
end

function string.count(text)
    -- 计算字符串宽度
    -- 可以计算出字符宽度，用于显示使用
   local lenInByte = #text
   local width = 0
   local i = 1
   while (i<=lenInByte) 
    do
        local curByte = string.byte(text, i)
        local byteCount = 1;
        if curByte>0 and curByte<=127 then
            byteCount = 1                                           --1字节字符
        elseif curByte>=192 and curByte<223 then
            byteCount = 2                                           --双字节字符
        elseif curByte>=224 and curByte<239 then
            byteCount = 3                                           --汉字
        elseif curByte>=240 and curByte<=247 then
            byteCount = 4                                           --4字节字符
        end

        local char = string.sub(text, i, i+byteCount-1)
        print(char)                                                         

        i = i + byteCount                                 -- 重置下一字节的索引
        local add =  byteCount >2 and 2 or 1 
        width = width + add                              -- 字符的个数（长度）
    end
    return width
end

local Num2Text = {
    cn = function (num)
        local text
        if num > 99999999 then
            text = math.floor(num / 100000000 + 0.5) .. _G.ENV.TEXT.nameNum100m
        elseif num > 99999 then
            text = math.floor(num / 10000 + 0.5) .. _G.ENV.TEXT.nameNum10k
        else
            text = tostring(num)
        end
        return text
    end,

    en = function (num)
        local text
        if num < 1000000 then
            text = StringFmt("{0:n0}", num)
        elseif num < 1000000000 then
            text = StringFmt("{0:n0}K", math.ceil(num / 1000))
        else
            text = StringFmt("{0:n0}M", math.ceil(num / 1000000))
        end
        return text
    end,
}
Num2Text.tw = Num2Text.cn

function string.show_num(num)
    local num2text = Num2Text[_G.lang] or Num2Text.en
    return num2text(num)
end
