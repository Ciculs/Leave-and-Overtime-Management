<script setup>
import { ref, computed, watch, onMounted } from "vue"
import { getHolidayByYear } from "@/services/holidayService"

const currentDate = ref(new Date())

const holidays = ref([])
const loadedYears = ref([])

const currentMonth = computed(() => currentDate.value.getMonth())
const currentYear = computed(() => currentDate.value.getFullYear())

const today = new Date()

const loadHoliday = async (year) => {
  if (loadedYears.value.includes(year)) return

  try {
    const res = await getHolidayByYear(year)

    holidays.value = [...holidays.value, ...res.data]
    loadedYears.value.push(year)
  } catch (err) {
    console.error("Holiday load error", err)
  }
}

onMounted(() => {
  loadHoliday(currentYear.value)
})

watch(currentYear, (year) => {
  loadHoliday(year)
})

const daysInMonth = computed(() => {
  return new Date(currentYear.value, currentMonth.value + 1, 0).getDate()
})

const firstDayOfMonth = computed(() => {

  let day = new Date(currentYear.value, currentMonth.value, 1).getDay()

  return day === 0 ? 6 : day - 1

})

const holidayMap = computed(() => {
  const map = {}

  holidays.value.forEach((h) => {
    map[h.holidayDate] = h.name
  })

  return map
})

const isWeekend = (day) => {
  const d = new Date(currentYear.value, currentMonth.value, day).getDay()
  return d === 0 || d === 6
}

const isToday = (day) => {
  return (
    day === today.getDate() &&
    currentMonth.value === today.getMonth() &&
    currentYear.value === today.getFullYear()
  )
}

const getDateKey = (day) => {
  return `${currentYear.value}-${String(currentMonth.value + 1).padStart(
    2,
    "0"
  )}-${String(day).padStart(2, "0")}`
}

const changeMonth = (offset) => {
  currentDate.value = new Date(
    currentYear.value,
    currentMonth.value + offset,
    1
  )
}
</script>

<template>
  <div class="calendar">

    <div class="calendar-header">
      <button @click="changeMonth(-1)">◀</button>

      <h3>
        {{
          new Date(currentYear, currentMonth).toLocaleString("default", {
            month: "long"
          })
        }}
        {{ currentYear }}
      </h3>

      <button @click="changeMonth(1)">▶</button>
    </div>

    <div class="calendar-grid">

      <div
        class="day-name"
        v-for="d in ['Sun','Mon','Tue','Wed','Thu','Fri','Sat']"
        :key="d"
      >
        {{ d }}
      </div>

      <div
        v-for="n in firstDayOfMonth"
        :key="'empty-' + n"
        class="empty-cell"
      ></div>

      <div
        v-for="day in daysInMonth"
        :key="day"
        class="day-cell"
        :class="{
          holiday: holidayMap[getDateKey(day)],
          weekend: isWeekend(day),
          today: isToday(day)
        }"
      >

        <div class="date-number">{{ day }}</div>

        <div
          v-if="holidayMap[getDateKey(day)]"
          class="holiday-name"
        >
          {{ holidayMap[getDateKey(day)] }}
        </div>

      </div>

    </div>

  </div>
</template>

<style scoped>

.calendar {
  margin-top: 20px;
}

.calendar-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 15px;
}

.calendar-header button {
  background: #eef2ff;
  border: none;
  padding: 6px 12px;
  border-radius: 8px;
  cursor: pointer;
}

.calendar-header button:hover {
  background: #dbe3ff;
}

.calendar-grid {
  display: grid;
  grid-template-columns: repeat(7, 1fr);
  gap: 8px;
}

.day-name {
  font-weight: bold;
  text-align: center;
  color: #707eae;
}

.day-cell {
  background: #f9fbff;
  border-radius: 12px;
  padding: 8px;
  min-height: 90px;
  transition: 0.2s;
  position: relative;
}

.day-cell:hover {
  background: #edf2ff;
}

.date-number {
  font-weight: bold;
}

.holiday {
  background: #ffecec;
  border: 1px solid #ffb3b3;
}

.holiday-name {
  font-size: 11px;
  margin-top: 5px;
  color: #d00000;
}

.weekend {
  background: #dfe6fa;
}

.today {
  border: 2px solid #6366f1;
}

.empty-cell {
  min-height: 90px;
}

</style>