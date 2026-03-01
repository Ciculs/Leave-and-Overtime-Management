<template>
  <h2>Top OT</h2>

  <div v-for="r in reports" :key="r.userName">
    {{ r.userName }} - {{ r.totalHours }} hours
  </div>
</template>

<script setup>
import { ref, onMounted } from "vue"
import api from "@/services/api"

const reports = ref([])

onMounted(async () => {
  const res = await api.get("/Report/top-ot")
  reports.value = res.data
})
</script>