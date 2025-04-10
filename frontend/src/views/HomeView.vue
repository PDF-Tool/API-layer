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
import { pageFormats, type PageFormat } from '@/lib/pageFormat'
import { Label } from '@/components/ui/label'

import { ref } from 'vue'

const currentFormat = ref<FileSizeFormat>('MB')
const currentPageFormat = ref<PageFormat>('A4')
const savePath = ref('')
</script>

<template>
  <section>
    <div class="flex flex-col gap-4">
      <div class="grid w-full items-center gap-2">
        <Label for="size">Size per page</Label>
        <div class="flex gap-2">
          <Input type="number" name="size" placeholder="Select image size per page..." />
          <DropdownMenu>
            <DropdownMenuTrigger as-child>
              <Button class="min-w-[70px] select-none"> {{ currentFormat }}
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
      <div class="grid w-full items-center gap-2">
        <Label for="size">Amount of pages</Label>
        <Input type="number" name="size" placeholder="Select amount of pages..." />
      </div>
      <div class="p-4 bg-primary rounded-lg border-input border shadow-sm flex flex-col gap-4">
        <div class="flex flex-col w-full items-center gap-2">
          <label class="w-full">Preset</label>
          <DropdownMenu class="w-full">
            <DropdownMenuTrigger as-child>
              <Button class="select-none bg-transparent border-1 text-black justify-start hover:bg-transparent  w-full">
                {{ currentPageFormat }}
                <span class="icon-[material-symbols--arrow-drop-down-rounded] text-xl"></span>
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent class="w-full">
              <DropdownMenuRadioGroup v-model="currentPageFormat">
                <DropdownMenuRadioItem v-for="format in pageFormats" :key="format" :value="format">
                  {{ format }}
                </DropdownMenuRadioItem>
              </DropdownMenuRadioGroup>
            </DropdownMenuContent>
          </DropdownMenu>
        </div>
        <div class="grid w-full items-center gap-2">
          <Label for="size">Dimensions</Label>
          <div class="flex gap-2">
            <div class="relative">
              <Input id="size" type="number" class="pl-7 bg-background!" />
              <span class="absolute start-0 inset-y-0 flex items-center justify-center px-2">
                <span class="text-foreground/50">W</span>
              </span>
            </div>
            <div class="relative">
              <Input id="size" type="number" class="pl-7 bg-background!" />
              <span class="absolute start-0 inset-y-0 flex items-center justify-center px-2">
                <span class="text-foreground/50">H</span>
              </span>
            </div>
            <DropdownMenu>
              <DropdownMenuTrigger as-child>
                <Button class="min-w-[70px] select-none"> {{ currentFormat }}
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
      </div>
      <div class="flex flex-col gap-4">
        <div class="flex flex-col gap-2">
          <Label for="checkHost">Check host connection</Label>
          <div class="flex gap-2">
            <Input id="checkHost" name="checkHost" type="text" placeholder="Enter host address..." />
            <Button>
              <span @click="checkHostConnection">Check</span>
            </Button>
          </div>
        </div>
      </div>
      <Button>Generate PDF</Button>
    </div>
  </section>
</template>