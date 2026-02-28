<template>
  <el-card>
    <h2>Register Overtime</h2>

    <el-form :model="form">
      <el-form-item label="Date">
        <el-date-picker v-model="form.date" type="date" />
      </el-form-item>

      <el-form-item label="Hours">
        <el-input-number v-model="form.hours" :min="1" />
      </el-form-item>

      <el-button type="primary" @click="submit">
        Submit
      </el-button>
    </el-form>
  </el-card>
</template>

<script setup>
import { reactive } from "vue"
import api from "@/services/api"

const form = reactive({
  date: "",
  hours: 1
})

const submit = async () => {
  await api.post("/ot", form)
  alert("OT Registered!")
}
</script>