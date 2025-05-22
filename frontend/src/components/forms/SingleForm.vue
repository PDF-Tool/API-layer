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
import { fileSizeFormats, metrics } from '@/lib/utils'
import { pageFormats } from '@/lib/pageFormat'
import { Label } from '@/components/ui/label'
import { computed, ref } from 'vue'
import { usePdfStore } from '@/stores/pdfStore'
import type { PdfFields } from '@/types/pdf'
import Info from '@/components/info.vue'
const { createAndPrintPdf } = usePdfStore()

const formData = ref<PdfFields>({
    Pages: 1,
    SizePerPage: 1,
    ByteUnit: 'MB',
    Host: '',
})

const canCreatePdf = computed(() => {
    return formData.value?.Pages && formData.value?.SizePerPage && formData.value?.Host
})
</script>

<template>
    <div class="flex flex-col gap-4">
        <div class="grid w-full items-center gap-2">
            <Label for="size">Size per page
                <Info
                    text="Select the amount of bytes per page in the PDF document, Select in the dropdown-menu in which size this needs to be." />
            </Label>
            <div class="flex gap-2">
                <Input type="number" v-model="formData.SizePerPage" name="sizePerPage"
                    placeholder="Select image size per page..." />
                <DropdownMenu>
                    <DropdownMenuTrigger as-child>
                        <Button class="min-w-[70px] select-none"> {{ formData.ByteUnit }}
                            <span class="icon-[material-symbols--arrow-drop-down-rounded] text-xl"></span>
                        </Button>
                    </DropdownMenuTrigger>
                    <DropdownMenuContent>
                        <DropdownMenuRadioGroup v-model="formData.ByteUnit">
                            <DropdownMenuRadioItem v-for="format in fileSizeFormats" :key="format" :value="format">
                                {{ format }}
                            </DropdownMenuRadioItem>
                        </DropdownMenuRadioGroup>
                    </DropdownMenuContent>
                </DropdownMenu>
            </div>
        </div>
        <div class="grid w-full items-center gap-2">
            <Label for="pages">Amount of pages
                <Info text="Select the amount of pages you want the PDF document to be." />
            </Label>
            <Input type="number" name="pages" v-model="formData.Pages" placeholder="Select amount of pages..." />
        </div>
        <div class="grid w-full items-center gap-2">
            <Label for="pages">Host/IP
                <Info text="Select the printer host address." />
            </Label>
            <Input type="text" name="Host" v-model="formData.Host" placeholder="Select host address..." />
        </div>
        <Button :disabled="!canCreatePdf" @click="createAndPrintPdf(formData)">Generate PDF</Button>
    </div>
</template>