<script setup lang="ts">
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuRadioGroup,
  DropdownMenuRadioItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'
import { fileSizeFormats, type FileSizeFormat } from '@/lib/utils'
import { Label } from '@/components/ui/label'

import { ref } from 'vue'

const currentFormat = ref<FileSizeFormat>('MB')

</script>

<template>
  <main>
    <div class="grid w-full items-center gap-2">
      <Label for="size">Size per page</Label>
      <div class="flex gap-2">
        <Input type="number" name="size" placeholder="Select image size per page..." />
        <DropdownMenu>
          <DropdownMenuTrigger>
            <Button class="min-w-[70px]"> {{ currentFormat }}
              <span class="icon-[material-symbols--arrow-drop-down-rounded] text-xl"></span>
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent>
            <DropdownMenuRadioGroup v-model="currentFormat">
              <DropdownMenuRadioItem v-for="format in fileSizeFormats" :key="format" :value="format">
                {{ format }}
              </DropdownMenuRadioItem>
            </DropdownMenuRadioGroup>
          </DropdownMenuContent>
        </DropdownMenu>
      </div>
    </div>
  </main>
</template>