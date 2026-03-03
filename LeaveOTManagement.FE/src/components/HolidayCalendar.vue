<script setup>
import { computed, ref } from "vue";

const props = defineProps({
  holidays: Array
});

const currentDate = ref(new Date());
const currentMonth = computed(() => currentDate.value.getMonth());
const currentYear = computed(() => currentDate.value.getFullYear());

const daysInMonth = computed(() => {
  return new Date(currentYear.value, currentMonth.value + 1, 0).getDate();
});

const firstDayOfMonth = computed(() => {
  return new Date(currentYear.value, currentMonth.value, 1).getDay();
});

const holidayMap = computed(() => {
  const map = {};
  props.holidays.forEach(h => {
    map[h.holidayDate] = h.name;
  });
  return map;
});

const changeMonth = (offset) => {
  currentDate.value = new Date(
    currentYear.value,
    currentMonth.value + offset,
    1
  );
};
</script>

<template>
  <div class="calendar">

    <div class="calendar-header">
      <button @click="changeMonth(-1)">◀</button>
      <h3>
        {{ new Date(currentYear, currentMonth).toLocaleString('default', { month: 'long' }) }}
        {{ currentYear }}
      </h3>
      <button @click="changeMonth(1)">▶</button>
    </div>

    <div class="calendar-grid">

      <div class="day-name" v-for="d in ['Sun','Mon','Tue','Wed','Thu','Fri','Sat']" :key="d">
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
          holiday: holidayMap[`${currentYear}-${String(currentMonth+1).padStart(2,'0')}-${String(day).padStart(2,'0')}`]
        }"
      >
        <div class="date-number">{{ day }}</div>

        <div
          v-if="holidayMap[`${currentYear}-${String(currentMonth+1).padStart(2,'0')}-${String(day).padStart(2,'0')}`]"
          class="holiday-name"
        >
          {{
            holidayMap[
              `${currentYear}-${String(currentMonth+1).padStart(2,'0')}-${String(day).padStart(2,'0')}`
            ]
          }}
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
  min-height: 80px;
  position: relative;
  transition: 0.2s;
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

.empty-cell {
  min-height: 80px;
}
</style>