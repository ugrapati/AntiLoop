param([string]$sz='64');$cert=gci cert:\ -re -c;function dll([string]$sz='64',[string]$name="Sean O'Neil Code")
{ $86=if($sz-eq'32'){'86'}else{$sz};if($cert.Count-gt1){$cert=$cert|?{$($_.Extensions|%{$_.Format(0)})-match'End Entity'};if($cert.Count-gt1){$cert=$cert|?{$_.DnsNameList.Unicode-eq$name}}}
  [string]$z="AntiLoop_$sz";[string]$s="C:\Projects\AntiLoop\x$86\Release\$z.dll"
  [string]$t="C:\Projects\AntiLoop\Resources\$z.bin"
  if(([IO.FileInfo]$s).Exists-and($b=$q::gf("\??\$s")))
  { $a=$q::gs($b)
    $i=$a.IndexOf($q::gs($q::unh('C1 E9 18 8B D1 8B E5 5D C3 CC')))
    if($i-ge0){$b[$i+8]=0xF3;$b[$i+9]=0xC3;$n=$n-bor1}
    $i=$a.IndexOf($q::gs($q::unh('FF FF FF 25')))
    if($i-ge0-and$b[$i+8]-eq0xCC-and$b[$i+9]-eq0xCC){$b[$i+3]=0x35;$b[$i+8]=0xF3;$b[$i+9]=0xC3;$n=$n-bor2}
    $i=$a.IndexOf($q::gs($q::unh('48 C1 E8 18 48 83 C4 28 C3 CC')))
    if($i-ge0){$b[$i+8]=0xF3;$b[$i+9]=0xC3;$n=$n-bor4}
    $i=$a.IndexOf($q::gs($q::unh('48 FF 25')))
    if($i-ge0-and$b[$i+7]-eq0xCC-and$b[$i+8]-eq0xCC){$b[$i+2]=0x35;$b[$i+7]=0xF3;$b[$i+8]=0xC3;$n=$n-bor8}
    if($n-eq3-or$n-eq12)
    { $pe=0x40;$hdr=$q::unh('4D 5A 40 00 03 00 00 00 04 00 00 00 CD 20 00 00 B8 00 00 00 00 00 00 00 0C 01 F0 FF')
      $ope=$q::r4($b,60);$ns=$q::r2($b,$ope+6);$so=24+$q::r2($b,$ope+20);$np=$so+$ns*40
      [Array]::Copy($hdr,0,$b,0,$hdr.Length);[Array]::Clear($b,28,32);$q::w4($b,60,$pe)
      [Array]::Copy($b,$ope,$b,$pe,$np);[Array]::Clear($b,$pe+$np,$ope-$pe)
      for($i=0;$i-lt$ns;$i++){$q::w8($b,$pe+$so+$i*40,0x6C69654E274F)}
    }else{$pe=$q::r4($b,60)} # $b[$pe+94]=$b[$pe+94]-bor0x80 # always verify signature - needs it to load into ms edge
    "$z : Got patched."
    # $b[$pe+$sz/2+104+14*8]=0x20;$b[$pe+$sz/2+108+14*8]=0x10
    [void]$q::sf("\??\$t",$b);pesum $t;$r=Set-AuthenticodeSignature $t $cert
    "$z : "+$r.StatusMessage;ri "C:\ProgramData\AntiLoop\$s.dll" -fo -re -errora 0
  }
} dll 32;dll 64
